// Başvuru sihirbazı: solda adımlar ("evrak yolu"), sağda o adımın formu.
// Her "Kaydet ve devam et" tıklamasında form backend'e kaydedilir;
// kullanıcı sonra geldiğinde kaldığı adımdan devam eder.

import { useCallback, useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { BASVURU_TURU_ADLARI, tarihSaat } from '../../api/etiketler'
import { hataMesaji } from '../../api/istemci'
import { basvuruApi, tanimApi } from '../../api/servisler'
import type { BasvuruDetay, BasvuruFormu, EksikAlan, Secenek } from '../../api/tipler'
import { Bildirim, DurumKasesi, GeriBaglantisi, telefonHatasi } from '../../bilesenler/Ortak'
import { useOturum } from '../../oturum/OturumBaglami'
import {
  Adim1Genel,
  Adim2Arastirmacilar,
  Adim3VeriToplama,
  Adim4DestekGider,
  Adim5Icerik,
  Adim6IsPaketleri,
  Adim7Etik,
} from './Adimlar'
import { ADIM_ADLARI } from './adimlar-listesi'
import { Adim9Belgeler } from './Belgeler'
import { Adim8Onizleme } from './Onizleme'

const FORM_ADIM_SAYISI = 7

export function BasvuruSayfasi() {
  const { id } = useParams()
  const basvuruId = Number(id)
  const { kullanici } = useOturum()

  const [basvuru, setBasvuru] = useState<BasvuruDetay | null>(null)
  const [form, setForm] = useState<BasvuruFormu | null>(null)
  const [secenekler, setSecenekler] = useState<Secenek[]>([])
  const [adim, setAdim] = useState(1)
  const [degisti, setDegisti] = useState(false)
  const [kaydediliyor, setKaydediliyor] = useState(false)
  const [sonKayit, setSonKayit] = useState<string | null>(null)
  const [hata, setHata] = useState<{ mesaj: string; ayrintilar: string[] } | null>(null)
  // Backend'in eksik kontrolü (her kayıttan sonra yenilenir); uyarılar ilgili alanın altında gösterilir
  const [eksikler, setEksikler] = useState<EksikAlan[]>([])

  // ───── Yükleme ─────
  const eksikleriYukle = useCallback(async (b: BasvuruDetay) => {
    if (!b.duzenlenebilirMi) {
      setEksikler([])
      return
    }
    try {
      const c = await basvuruApi.eksikler(b.id)
      setEksikler(c.eksikler)
    } catch {
      /* eksik kontrolü alınamazsa form yine de kullanılabilir */
    }
  }, [])

  const yenile = useCallback(async () => {
    const b = await basvuruApi.getir(basvuruId)
    setBasvuru(b)
    setForm(b)
    setDegisti(false)
    await eksikleriYukle(b)
  }, [basvuruId, eksikleriYukle])

  useEffect(() => {
    Promise.all([basvuruApi.getir(basvuruId), tanimApi.secenekler()])
      .then(([b, s]) => {
        setBasvuru(b)
        setForm(b)
        setSecenekler(s)
        // Kaldığı yerden devam: düzenlenebiliyorsa son adım, değilse belgeler adımı
        setAdim(b.duzenlenebilirMi ? Math.min(Math.max(b.sonAdim, 1), 8) : 9)
        void eksikleriYukle(b)
      })
      .catch((e) => setHata(hataMesaji(e)))
  }, [basvuruId, eksikleriYukle])

  // Kaydedilmemiş değişiklik varken sayfadan çıkılırsa uyar
  useEffect(() => {
    if (!degisti) return
    const uyar = (e: BeforeUnloadEvent) => e.preventDefault()
    window.addEventListener('beforeunload', uyar)
    return () => window.removeEventListener('beforeunload', uyar)
  }, [degisti])

  const eksiklerDegisti = useCallback((e: EksikAlan[]) => setEksikler(e), [])

  if (hata && !basvuru) return <Bildirim tur="hata" mesaj={hata.mesaj} ayrintilar={hata.ayrintilar} />
  if (!basvuru || !form || !kullanici) return <p className="bos">Başvuru yükleniyor…</p>

  const duzenlenebilir = basvuru.duzenlenebilirMi

  // Bir adımın uyarıları, kullanıcı o adımı geçtikten sonra (ya da önizlemeye geldikten sonra) gösterilir;
  // böylece forma ilk başlayan kişi kırmızı uyarılarla karşılaşmaz.
  const uyariGosterilsinMi = (n: number) => duzenlenebilir && (n < basvuru.sonAdim || basvuru.sonAdim >= 8)
  const uyari = (alan: string) =>
    eksikler.filter((e) => e.alan === alan && uyariGosterilsinMi(e.adim)).map((e) => e.mesaj)
  const eksikAdimlar = new Set(eksikler.filter((e) => uyariGosterilsinMi(e.adim)).map((e) => e.adim))

  function guncelle(parca: Partial<BasvuruFormu>) {
    setForm((f) => (f ? { ...f, ...parca } : f))
    setDegisti(true)
  }

  /** Formu kaydeder; gidilecekAdim verilirse kayıttan sonra oraya geçer */
  async function kaydet(gidilecekAdim?: number) {
    if (!form || !basvuru) return
    // İleri gidilirken "son adım" da kaydedilsin (değişiklik olmasa bile), ki kullanıcı oradan devam etsin
    const ilerliyor = gidilecekAdim !== undefined && gidilecekAdim > form.sonAdim && gidilecekAdim <= 8
    if (!duzenlenebilir || (!degisti && !ilerliyor)) {
      if (gidilecekAdim) setAdim(gidilecekAdim)
      return
    }
    // Telefon numarası kuralına uymayan araştırmacı varsa kaydetme
    const hataliTelefon = form.arastirmacilar.find((a) => telefonHatasi(a.telefon))
    if (hataliTelefon) {
      setHata({ mesaj: `${hataliTelefon.adSoyad || 'Bir araştırmacı'}: ${telefonHatasi(hataliTelefon.telefon)}`, ayrintilar: [] })
      setAdim(2)
      return
    }
    setKaydediliyor(true)
    setHata(null)
    try {
      const sonAdim = Math.max(form.sonAdim, gidilecekAdim ?? adim)
      const kayitli = await basvuruApi.kaydet(basvuru.id, { ...form, sonAdim })
      setBasvuru(kayitli)
      setForm(kayitli) // yeni araştırmacılar artık Id'leriyle geliyor
      setDegisti(false)
      setSonKayit(new Date().toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' }))
      if (gidilecekAdim) setAdim(gidilecekAdim)
      window.scrollTo({ top: 0 })
      void eksikleriYukle(kayitli)
    } catch (e) {
      setHata(hataMesaji(e))
    } finally {
      setKaydediliyor(false)
    }
  }

  function adimaGit(n: number) {
    if (n === adim) return
    void kaydet(n)
    window.scrollTo({ top: 0 })
  }

  const adimProps = { form, guncelle, uyari, duzenlenebilir }

  return (
    <>
      <GeriBaglantisi to="/basvurular" metin="Başvurularım" />
      <div className="sayfa-basligi">
        <div>
          <p>
            Başvuru no {basvuru.id} · {BASVURU_TURU_ADLARI[basvuru.basvuruTuru]} · Adım {adim} / {ADIM_ADLARI.length}
          </p>
          <h1 style={{ marginTop: 6 }}>{form.arastirmaAdi || 'Adsız başvuru'}</h1>
        </div>
        <DurumKasesi durum={basvuru.durum} buyuk />
      </div>

      <div className="sihirbaz">
        <nav aria-label="Başvuru adımları">
          <ol className="adim-yolu">
            {ADIM_ADLARI.map((ad, i) => {
              const n = i + 1
              const siniflar = [
                n === adim ? 'aktif' : '',
                n <= basvuru.sonAdim && n !== adim ? 'gecildi' : '',
                eksikAdimlar.has(n) ? 'eksik' : '',
              ].join(' ')
              return (
                <li key={n} className={siniflar}>
                  <button onClick={() => adimaGit(n)} aria-current={n === adim ? 'step' : undefined}>
                    <span className="no">{n}</span>
                    {ad}
                  </button>
                </li>
              )
            })}
          </ol>
        </nav>

        <section className="kart" aria-live="polite">
          {hata && <Bildirim tur="hata" mesaj={hata.mesaj} ayrintilar={hata.ayrintilar} />}

          {!duzenlenebilir && adim <= FORM_ADIM_SAYISI && (
            <Bildirim
              tur="bilgi"
              mesaj={
                basvuru.durum === 'ImzaliBelgeBekleniyor'
                  ? 'Form kilitli, sadece görüntüleyebilirsiniz. Değiştirmek için 9. adımdan kilidi açın.'
                  : `Başvuru ${tarihSaat(basvuru.gonderimTarihi)} tarihinde gönderildi; artık değiştirilemez.`
              }
            />
          )}

          {adim <= FORM_ADIM_SAYISI ? (
            <fieldset disabled={!duzenlenebilir}>
              {adim === 1 && <Adim1Genel {...adimProps} />}
              {adim === 2 && <Adim2Arastirmacilar {...adimProps} />}
              {adim === 3 && <Adim3VeriToplama {...adimProps} secenekler={secenekler} />}
              {adim === 4 && <Adim4DestekGider {...adimProps} />}
              {adim === 5 && <Adim5Icerik {...adimProps} />}
              {adim === 6 && <Adim6IsPaketleri {...adimProps} />}
              {adim === 7 && <Adim7Etik {...adimProps} secenekler={secenekler} />}
            </fieldset>
          ) : adim === 8 ? (
            <Adim8Onizleme basvuru={basvuru} adimaGit={adimaGit} eksiklerDegisti={eksiklerDegisti} yenile={yenile} />
          ) : (
            <Adim9Belgeler basvuru={basvuru} adimaGit={adimaGit} yenile={yenile} />
          )}

          {adim <= FORM_ADIM_SAYISI && (
            <div className="form-alt">
              <button className="dugme metin" onClick={() => adimaGit(adim - 1)} disabled={adim === 1 || kaydediliyor}>
                Önceki adım
              </button>
              <div className="dugmeler">
                {duzenlenebilir && (
                  <span className="kayit-durumu" aria-live="polite">
                    {kaydediliyor
                      ? 'Kaydediliyor…'
                      : degisti
                        ? 'Kaydedilmemiş değişiklik var'
                        : sonKayit
                          ? `Kaydedildi (${sonKayit})`
                          : ''}
                  </span>
                )}
                {duzenlenebilir && (
                  <button className="dugme ikincil" onClick={() => kaydet()} disabled={kaydediliyor || !degisti}>
                    Kaydet
                  </button>
                )}
                <button className="dugme" onClick={() => adimaGit(adim + 1)} disabled={kaydediliyor}>
                  {duzenlenebilir ? 'Kaydet ve devam et' : 'Sonraki adım'}
                </button>
              </div>
            </div>
          )}
        </section>
      </div>
    </>
  )
}
