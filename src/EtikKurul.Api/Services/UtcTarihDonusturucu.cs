using System.Text.Json;
using System.Text.Json.Serialization;

namespace EtikKurul.Api.Services;

/// <summary>
/// Veritabanında bütün tarihler UTC saklanır. Ama SQL Server'dan okunan tarih
/// "hangi saat dilimi" bilgisini kaybeder ve JSON'da sonunda 'Z' olmadan çıkar.
/// Frontend o zaman saati 3 saat kaymış gösterebilir.
/// Bu dönüştürücü JSON'a yazarken her tarihi UTC olarak işaretler (sonuna 'Z' koyar).
/// </summary>
public class UtcTarihDonusturucu : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tarih = reader.GetDateTime();
        return tarih.Kind switch
        {
            DateTimeKind.Local => tarih.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(tarih, DateTimeKind.Utc),
            _ => tarih,
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utc);
    }
}

/// <summary>UTC tarihi Türkiye saatine çevirir (PDF'lerde ve e-postalarda göstermek için).</summary>
public static class TurkiyeSaati
{
    private static readonly TimeZoneInfo Bolge = BolgeyiBul();

    private static TimeZoneInfo BolgeyiBul()
    {
        foreach (var id in new[] { "Europe/Istanbul", "Turkey Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("TR", TimeSpan.FromHours(3), "Türkiye", "Türkiye");
    }

    public static DateTime Cevir(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Bolge);

    public static DateTime Simdi => Cevir(DateTime.UtcNow);
}
