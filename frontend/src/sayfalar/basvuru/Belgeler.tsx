// 9. adım: ıslak imzalı belgeleri yükleme ve başvuruyu gönderme.

import { useCallback, useEffect, useState } from 'react'
import { tarihSaat } from '../../api/etiketler'
import { hataMesaji } from '../../api/istemci'
import { basvuruApi } from '../../api/servisler'
import type { Arastirmaci, BasvuruDetay, GerekliEk, YuklenenEk } from '../../api/tipler'
import { Bildirim } from '../../bilesenler/Ortak'

const MAKS_MB = 10

export function Adim9Belgeler({
  basvuru,
  adimaGit,
  yenile,
}: {
  basvuru: BasvuruDetay
  adimaGit: (n: number) => void
  yenile: () => Promise<void>
}) {
  const [gerekliler, setGerekliler] = useState<GerekliEk[] | null>(null)
  const [yuklenenler, setYuklenenler] = useState<YuklenenEk[]>([])
  const [mesaj, setMesaj] = useState<{ tur: 'hata' | 'basari' | 'bilgi'; metin: string; ayrintilar?: string[] } | null>(
    null,
  )
  const [isleniyor, setIsleniyor] = useState(false)

  const belgeAsamasi = basvuru.durum === 'ImzaliBelgeBekleniyor'
  const gonderildi = !['Taslak', 'ImzaliBelgeBekleniyor', 'IadeEdildi'].includes(basvuru.durum)

  const listeleriYukle = useCallback(async () => {
    const [g, y] = await Promise.all([basvuruApi.gerekliEkler(basvuru.id), basvuruApi.ekler(basvuru.id)])
    setGerekliler(g)
    setYuklenenler(y)
  }, [basvuru.id])

  useEffect(() => {
    if (basvuru.durum === 'Taslak') return
    listeleriYukle().catch((e) => setMesaj({ tur: 'hata', metin: hataMesaji(e).mesaj }))
  }, [basvuru.durum, listeleriYukle])

  async function islem(fn: () => Promise<unknown>, basariMetni?: string) {
    setIsleniyor(true)
    setMesaj(null)
    try {
      await fn()
      await listeleriYukle()
      if (basariMetni) setMesaj({ tur: 'basari', metin: basariMetni })
    } catch (e) {
      const h = hataMesaji(e)
      setMesaj({ tur: 'hata', metin: h.mesaj, ayrintilar: h.ayrintilar })
    } finally {
      setIsleniyor(false)
    }
  }

  async function yukle(ek: GerekliEk, dosya: File, arastirmaciId?: number) {
    if (!dosya.name.toLowerCase().endsWith('.pdf')) {
      setMesaj({ tur: 'hata', metin: 'Sadece PDF dosyası yükleyebilirsiniz.' })
      return
    }
    if (dosya.size > MAKS_MB * 1024 * 1024) {
      setMesaj({ tur: 'hata', metin: `Dosya en fazla ${MAKS_MB} MB olabilir.` })
      return
    }
    await islem(() => basvuruApi.ekYukle(basvuru.id, ek.ekTuruId, dosya, arastirmaciId), `${ek.ad} yüklendi.`)
  }

  async function sil(y: YuklenenEk) {
    if (!confirm(`"${y.orijinalDosyaAdi}" silinsin mi?`)) return
    await islem(() => basvuruApi.ekSil(basvuru.id, y.id), 'Belge silindi.')
  }

  async function ac(y: YuklenenEk) {
    try {
      await basvuruApi.ekAc(basvuru.id, y.id)
    } catch (e) {
      setMesaj({ tur: 'hata', metin: hataMesaji(e).mesaj })
    }
  }

  async function gonder() {
    if (!confirm('Başvurunuz evrak kayda gönderilecek. Gönderdikten sonra belgeleri değiştiremezsiniz.\n\nGönderilsin mi?'))
      return
    setIsleniyor(true)
    setMesaj(null)
    try {
      await basvuruApi.gonder(basvuru.id)
      await yenile()
    } catch (e) {
      const h = hataMesaji(e)
      setMesaj({ tur: 'hata', metin: h.mesaj, ayrintilar: h.ayrintilar })
      setIsleniyor(false)
    }
  }

  async function kilidiAc() {
    if (
      !confirm(
        'Kilidi açarsanız form tekrar düzenlenebilir olur, ancak YÜKLEDİĞİNİZ BÜTÜN BELGELER SİLİNİR. ' +
          'Değişiklikten sonra yeni çıktı alıp tekrar imzalamanız gerekir.\n\nKilit açılsın mı?',
      )
    )
      return
    setIsleniyor(true)
    try {
      await basvuruApi.kilidiAc(basvuru.id)
      await yenile()
      adimaGit(1)
    } catch (e) {
      setMesaj({ tur: 'hata', metin: hataMesaji(e).mesaj })
      setIsleniyor(false)
    }
  }

  // ───── Henüz kilitlenmemiş ─────
  if (basvuru.durum === 'Taslak') {
    return (
      <>
        <h2>Belgeler ve gönderim</h2>
        <Bildirim tur="bilgi" mesaj="Belgeleri yüklemek için önce formu tamamlayıp 8. adımda kilitlemeniz gerekiyor." />
        <button className="dugme" onClick={() => adimaGit(8)}>
          Önizleme ve çıktı adımına git
        </button>
      </>
    )
  }

  const zorunlularTamam = gerekliler?.every((g) => !g.zorunluMu || g.tamamMi) ?? false

  return (
    <>
      <h2>Belgeler ve gönderim</h2>

      {gonderildi ? (
        <Bildirim
          tur="basari"
          mesaj={`Başvurunuz ${tarihSaat(basvuru.gonderimTarihi)} tarihinde gönderildi.${
            basvuru.evrakKayitNo ? ` Evrak kayıt numarası: ${basvuru.evrakKayitNo}.` : ' Evrak kaydı bekleniyor.'
          }`}
        />
      ) : (
        <ol style={{ paddingLeft: 20, marginTop: 0 }}>
          <li>
            Başvuru paketini indirip yazdırın.{' '}
            <button className="dugme metin kucuk" onClick={() => basvuruApi.pdfAc(basvuru.id).catch(() => {})}>
              PDF'i aç
            </button>
          </li>
          <li>Islak imzayla imzalayın (tez çalışmalarında danışman ve tez yazarı).</li>
          <li>Her belgeyi ayrı PDF olarak taratın ve aşağıdan yükleyin.</li>
          <li>Zorunlu belgelerin hepsi tamamlanınca başvurunuzu gönderin.</li>
        </ol>
      )}

      {mesaj && <Bildirim tur={mesaj.tur} mesaj={mesaj.metin} ayrintilar={mesaj.ayrintilar} />}

      {gerekliler === null ? (
        <p className="bos">Belgeler yükleniyor…</p>
      ) : (
        <ul className="ek-listesi">
          {gerekliler.map((ek) => (
            <EkSatiri
              key={ek.ekTuruId}
              ek={ek}
              yuklenenler={yuklenenler.filter((y) => y.ekTuruId === ek.ekTuruId)}
              arastirmacilar={basvuru.arastirmacilar}
              duzenlenebilir={belgeAsamasi && !isleniyor}
              yukle={yukle}
              sil={sil}
              ac={ac}
            />
          ))}
        </ul>
      )}

      {belgeAsamasi && (
        <div className="form-alt">
          <button className="dugme tehlike" onClick={kilidiAc} disabled={isleniyor}>
            Kilidi aç ve formu düzenle
          </button>
          <div className="dugmeler">
            {!zorunlularTamam && <span className="kayit-durumu">Zorunlu belgelerin hepsi yüklenince gönderebilirsiniz.</span>}
            <button className="dugme" onClick={gonder} disabled={isleniyor || !zorunlularTamam}>
              Başvuruyu gönder
            </button>
          </div>
        </div>
      )}
    </>
  )
}

// ───── Tek bir ek türünün satırı ─────

function EkSatiri({
  ek,
  yuklenenler,
  arastirmacilar,
  duzenlenebilir,
  yukle,
  sil,
  ac,
}: {
  ek: GerekliEk
  yuklenenler: YuklenenEk[]
  arastirmacilar: Arastirmaci[]
  duzenlenebilir: boolean
  yukle: (ek: GerekliEk, dosya: File, arastirmaciId?: number) => Promise<void>
  sil: (y: YuklenenEk) => void
  ac: (y: YuklenenEk) => void
}) {
  const kisiBasi = ek.kod === 'EK7' // her araştırmacı için ayrı özgeçmiş

  const durum = !ek.zorunluMu ? (
    <span className="ek-durum istege">İsteğe bağlı</span>
  ) : ek.tamamMi ? (
    <span className="ek-durum tamam">Tamam{ek.gerekenAdet > 1 ? ` (${ek.yuklenenAdet}/${ek.gerekenAdet})` : ''}</span>
  ) : (
    <span className="ek-durum eksik">
      Eksik{ek.gerekenAdet > 1 ? ` (${ek.yuklenenAdet}/${ek.gerekenAdet})` : ''}
    </span>
  )

  return (
    <li>
      <div className="ek-baslik">
        <strong>{ek.ad}</strong>
        {durum}
      </div>
      <p className="ek-neden">
        {ek.neden}
        {ek.sistemUretirMi && ' Sistemin ürettiği PDF\'te bulunur; imzalayıp taratın.'}
      </p>

      {kisiBasi ? (
        <ul className="yuklenenler">
          {arastirmacilar.map((a) => {
            const y = yuklenenler.find((x) => x.basvuruArastirmacisiId === a.id)
            return (
              <li key={a.id}>
                <span style={{ minWidth: 180 }}>{a.adSoyad}</span>
                {y ? (
                  <YuklenenDosya y={y} duzenlenebilir={duzenlenebilir} sil={sil} ac={ac} />
                ) : (
                  <span className="ek-durum eksik">Yüklenmedi</span>
                )}
                {duzenlenebilir && (
                  <DosyaSec etiket={y ? 'Değiştir' : 'Yükle'} sec={(d) => yukle(ek, d, a.id)} />
                )}
              </li>
            )
          })}
        </ul>
      ) : (
        <>
          {yuklenenler.length > 0 && (
            <ul className="yuklenenler">
              {yuklenenler.map((y) => (
                <li key={y.id}>
                  <YuklenenDosya y={y} duzenlenebilir={duzenlenebilir} sil={sil} ac={ac} />
                </li>
              ))}
            </ul>
          )}
          {duzenlenebilir && (
            <DosyaSec
              etiket={yuklenenler.length > 0 && !ek.cokluDosyaMi ? 'Değiştir' : ek.cokluDosyaMi && yuklenenler.length ? 'Bir dosya daha ekle' : 'PDF yükle'}
              sec={(d) => yukle(ek, d)}
            />
          )}
        </>
      )}
    </li>
  )
}

function YuklenenDosya({
  y,
  duzenlenebilir,
  sil,
  ac,
}: {
  y: YuklenenEk
  duzenlenebilir: boolean
  sil: (y: YuklenenEk) => void
  ac: (y: YuklenenEk) => void
}) {
  return (
    <>
      <button className="dugme metin kucuk" onClick={() => ac(y)} title="Yeni sekmede aç">
        {y.orijinalDosyaAdi}
      </button>
      <span style={{ color: 'var(--soluk)' }}>
        {(y.boyutByte / 1024).toFixed(0)} KB · {tarihSaat(y.yuklemeTarihi)}
      </span>
      {duzenlenebilir && (
        <button className="dugme metin kucuk" onClick={() => sil(y)} style={{ color: 'var(--kase-kirmizi)' }}>
          Sil
        </button>
      )}
    </>
  )
}

/** Gizli dosya kutusu + görünen düğme */
function DosyaSec({ etiket, sec }: { etiket: string; sec: (d: File) => void }) {
  return (
    <label className="dugme ikincil kucuk" style={{ cursor: 'pointer' }}>
      {etiket}
      <input
        type="file"
        accept="application/pdf,.pdf"
        className="gizli-baslik"
        onChange={(e) => {
          const d = e.target.files?.[0]
          if (d) sec(d)
          e.target.value = '' // aynı dosya tekrar seçilebilsin
        }}
      />
    </label>
  )
}
