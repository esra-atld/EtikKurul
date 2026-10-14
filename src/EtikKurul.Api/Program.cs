using System.Text;
using System.Text.Json.Serialization;
using EtikKurul.Api.Data;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Services;
using EtikKurul.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ───────────── 1) VERİTABANI ─────────────
builder.Services.AddDbContext<AppDbContext>(secenek =>
    secenek.UseSqlServer(builder.Configuration.GetConnectionString("Varsayilan")));

// ───────────── 2) KULLANICI VE ROL SİSTEMİ (Identity) ─────────────
builder.Services
    .AddIdentityCore<Kullanici>(secenek =>
    {
        secenek.User.RequireUniqueEmail = true;

        secenek.Password.RequiredLength = 8;
        secenek.Password.RequireDigit = true;
        secenek.Password.RequireUppercase = true;
        secenek.Password.RequireLowercase = true;
        secenek.Password.RequireNonAlphanumeric = false;

        // 5 hatalı şifre denemesinde hesap 5 dakika kilitlenir.
        secenek.Lockout.MaxFailedAccessAttempts = 5;
        secenek.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<TurkceIdentityHatalari>()
    .AddDefaultTokenProviders();

// ───────────── 3) GİRİŞ (JWT) ─────────────
var jwtBolumu = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtAyarlari>(jwtBolumu);
var jwt = jwtBolumu.Get<JwtAyarlari>()
          ?? throw new InvalidOperationException("appsettings içinde 'Jwt' bölümü bulunamadı.");
if (jwt.Anahtar.Length < 32)
    throw new InvalidOperationException("Jwt:Anahtar en az 32 karakter olmalı.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(secenek =>
    {
        // Token içindeki "sub", "role" gibi adları olduğu gibi kullan (uzun adlara çevirme).
        secenek.MapInboundClaims = false;
        secenek.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Yayinci,
            ValidateAudience = true,
            ValidAudience = jwt.Hedef,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Anahtar)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role", // [Authorize(Roles = ...)] bu alana bakar
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenServisi>();

// ───────────── 4) FRONTEND ERİŞİMİ (CORS) ─────────────
// Frontend farklı bir adreste (ör. React: localhost:5173) çalışacağı için izin veriyoruz.
var frontendAdresleri = builder.Configuration.GetSection("FrontendAdresleri").Get<string[]>() ?? [];
builder.Services.AddCors(secenek =>
    secenek.AddPolicy("Frontend", politika => politika
        .WithOrigins(frontendAdresleri)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ───────────── 5) CONTROLLER'LAR VE SWAGGER ─────────────
builder.Services
    .AddControllers()
    // Enum'lar JSON'da sayı değil metin olsun: "durum": "Taslak"
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(secenek =>
{
    secenek.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Etik Kurul Otomasyonu API",
        Version = "v1",
        Description = "Selçuk Üniversitesi Etik Kurul Başvuru Otomasyonu",
    });

    // Swagger'daki "Authorize" düğmesi: giriş yapıp aldığınız token'ı buraya yapıştırırsınız.
    secenek.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Giriş yapınca dönen token'ı yapıştırın (başına 'Bearer' yazmayın).",
    });
    secenek.AddSecurityRequirement(belge => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", belge)] = [],
    });
});

var app = builder.Build();

// ───────────── 6) AÇILIŞTA: MIGRATION + ROLLER + ADMIN ─────────────
using (var kapsam = app.Services.CreateScope())
{
    var servisler = kapsam.ServiceProvider;
    var logger = servisler.GetRequiredService<ILogger<Program>>();

    if (app.Environment.IsDevelopment())
    {
        // Geliştirme ortamında bekleyen migration'ları otomatik uygula.
        await servisler.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    await BaslangicVerisi.YukleAsync(servisler, app.Configuration, logger);
}

// ───────────── 7) İSTEK HATTI (sıra önemli) ─────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(secenek => secenek.DocumentTitle = "Etik Kurul API");
}
else
{
    app.UseHttpsRedirection(); // Canlı ortamda http → https yönlendir.
}

app.UseCors("Frontend");
app.UseAuthentication(); // Önce "kimsin?"
app.UseAuthorization();  // Sonra "yetkin var mı?"
app.MapControllers();

app.Run();
