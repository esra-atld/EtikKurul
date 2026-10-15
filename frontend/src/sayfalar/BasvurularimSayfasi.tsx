import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { BASVURU_TURU_ADLARI, tarihSaat } from '../api/etiketler'
import { hataMesaji } from '../api/istemci'
import { basvuruApi } from '../api/servisler'
import type { BasvuruOzet } from '../api/tipler'
import { Alan, Bildirim, DurumKasesi, GeriBaglantisi } from '../bilesenler/Ortak'

export function BasvurularimSayfasi() {
  const navigate = useNavigate()
  const [parametreler, setParametreler] = useSearchParams()
  const [liste, setListe] = useState<BasvuruOzet[] | null>(null)
  const [hata, setHata] = useState<string | null>(null)
  const [yeniAd, setYeniAd] = useState('')
  const [olusturuluyor, setOlusturuluyor] = useState(false)

  // Ana sayfadaki "Yeni başvuru başlat" buraya ?yeni=1 ile gelir
  const yeniAcik = parametreler.get('yeni') === '1'
  const yeniFormunuAc = (acik: boolean) => setParametreler(acik ? { yeni: '1' } : {})

  function yukle() {
    setHata(null)
    basvuruApi
      .benimkiler()
      .then(setListe)
      .catch((e) => {
        setListe([])
        setHata(hataMesaji(e).mesaj)
      })
  }
  useEffect(yukle, [])

  async function olustur(e: FormEvent) {
    e.preventDefault()
    if (!yeniAd.trim()) {
      setHata('Araştırmanın adını yazın.')
      return
    }
    setOlusturuluyor(true)
    try {
      const b = await basvuruApi.olustur(yeniAd.trim())
      navigate(`/basvurular/${b.id}`)
    } catch (err) {
      setHata(hataMesaji(err).mesaj)
      setOlusturuluyor(false)
    }
  }

  async function sil(b: BasvuruOzet) {
    if (!confirm(`"${b.arastirmaAdi || 'Adsız taslak'}" taslağı silinsin mi? Bu işlem geri alınamaz.`)) return
    try {
      await basvuruApi.sil(b.id)
      yukle()
    } catch (err) {
      setHata(hataMesaji(err).mesaj)
    }
  }

  return (
    <>
      <GeriBaglantisi to="/" metin="Ana sayfa" />
      <div className="sayfa-basligi">
        <div>
          <h1>Başvurularım</h1>
          <p>Taslaklarınıza kaldığınız yerden devam edin, gönderdiğiniz başvuruların nerede olduğunu görün.</p>
        </div>
        {/* Sayfadaki TEK "yeni başvuru" düğmesi */}
        {!yeniAcik && (
          <button className="dugme" onClick={() => yeniFormunuAc(true)}>
            Yeni başvuru başlat
          </button>
        )}
      </div>

      {hata && <Bildirim tur="hata" mesaj={hata} />}

      {yeniAcik && (
        <form className="kart" onSubmit={olustur} style={{ marginBottom: 16 }}>
          <h2>Yeni başvuru</h2>
          <Alan etiket="Araştırmanın adı" ipucu="Proje başvurusundaki konu başlığıyla aynı olmalı. Sonra değiştirebilirsiniz.">
            <input type="text" value={yeniAd} onChange={(e) => setYeniAd(e.target.value)} autoFocus required />
          </Alan>
          <div className="dugmeler">
            <button className="dugme" disabled={olusturuluyor}>
              {olusturuluyor ? 'Oluşturuluyor…' : 'Başvuruyu oluştur ve forma geç'}
            </button>
            <button type="button" className="dugme metin" onClick={() => yeniFormunuAc(false)}>
              Vazgeç
            </button>
          </div>
        </form>
      )}

      <div className="kart">
        {liste === null && <p className="bos">Başvurularınız yükleniyor…</p>}

        {liste?.length === 0 && !hata && (
          <Bildirim
            tur="bilgi"
            mesaj="Henüz bir başvurunuz bulunmuyor. Başvuru yapmak için sağ üstteki 'Yeni başvuru başlat' düğmesini kullanın."
          />
        )}

        {liste && liste.length > 0 && (
          <ul className="basvuru-listesi">
            {liste.map((b) => (
              <li key={b.id}>
                <div>
                  <h3>
                    <Link to={`/basvurular/${b.id}`}>{b.arastirmaAdi || 'Adsız taslak'}</Link>
                  </h3>
                  <div className="meta">
                    Başvuru no {b.id} · {BASVURU_TURU_ADLARI[b.basvuruTuru]}
                    {b.evrakKayitNo && <> · Evrak kayıt no {b.evrakKayitNo}</>}
                    <br />
                    {b.gonderimTarihi
                      ? `Gönderildi: ${tarihSaat(b.gonderimTarihi)}`
                      : `Son kayıt: ${tarihSaat(b.guncellemeTarihi)} · ${b.sonAdim}. adımda kaldınız`}
                  </div>
                </div>
                <div className="dugmeler">
                  <DurumKasesi durum={b.durum} />
                  <Link className="dugme ikincil kucuk" to={`/basvurular/${b.id}`}>
                    {b.durum === 'Taslak' ? 'Devam et' : 'Aç'}
                  </Link>
                  {b.durum === 'Taslak' && (
                    <button className="dugme metin kucuk" onClick={() => sil(b)}>
                      Sil
                    </button>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </>
  )
}
