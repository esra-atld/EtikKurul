using EtikKurul.Api.Data;
using EtikKurul.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Controllers;

/// <summary>Formdaki sabit listeler. Frontend bunları kutucukları çizmek için kullanır.</summary>
[ApiController]
[Route("api/tanimlar")]
[Authorize]
public class TanimlarController(AppDbContext db) : ControllerBase
{
    /// <summary>Çoklu seçim seçenekleri (veri toplama aracı, katılımcı grupları, uygulamalar).</summary>
    [HttpGet("secenekler")]
    public async Task<ActionResult<List<SecenekDto>>> Secenekler() =>
        await db.Secenekler.AsNoTracking()
            .Where(s => s.AktifMi)
            .OrderBy(s => s.Grup).ThenBy(s => s.Sira)
            .Select(s => new SecenekDto(s.Id, s.Grup, s.Ad, s.Sira, s.AciklamaGerekirMi, s.CocukRizasiGerektirirMi))
            .ToListAsync();

    /// <summary>Belge türleri: Dilekçe, Başvuru Formu, Ek-1 ... Ek-11.</summary>
    [HttpGet("ek-turleri")]
    public async Task<ActionResult<List<EkTuruDto>>> EkTurleri() =>
        await db.EkTurleri.AsNoTracking()
            .Where(t => t.AktifMi)
            .OrderBy(t => t.Sira)
            .Select(t => new EkTuruDto(t.Id, t.Kod, t.Ad, t.Aciklama, t.ZorunlulukKurali, t.CokluDosyaMi, t.SistemUretirMi))
            .ToListAsync();
}
