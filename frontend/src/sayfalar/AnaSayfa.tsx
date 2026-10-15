// Giriş yaptıktan sonra açılan ilk sayfa.

import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ROL_ADLARI, tarihSaat } from '../api/etiketler'
import { hataMesaji } from '../api/istemci'
import { basvuruApi } from '../api/servisler'
import type { BasvuruOzet } from '../api/tipler'
import { Bildirim, DurumKasesi } from '../bilesenler/Ortak'
import { useOturum } from '../oturum/OturumBaglami'

export function AnaSayfa() {
  const { kullanici } = useOturum()
  const navigate = useNavigate()
  const [liste, setListe] = useState<BasvuruOzet[] | null>(null)
  const [hata, setHata] = useState<string | null>(null)

  useEffect(() => {
    basvuruApi
      .benimkiler()
      .then(setListe)
      .catch((e) => setHata(hataMesaji(e).mesaj))
  }, [])

  if (!kullanici) return null

  const taslaklar = liste?.filter((b) => b.durum === 'Taslak' || b.durum === 'ImzaliBelgeBekleniyor') ?? []
  const gonderilenler = liste?.filter((b) => b.durum !== 'Taslak' && b.durum !== 'ImzaliBelgeBekleniyor') ?? []
  const sonTaslak = taslaklar[0]

  return (
    <>
      <div className="sayfa-basligi">
        <div>
          <h1>Hoş geldiniz, {kullanici.ad}</h1>
          <p>Etik kurul başvurularınızı buradan başlatıp takip edebilirsiniz.</p>
        </div>
      </div>

      {hata && <Bildirim tur="hata" mesaj={hata} />}

      <div className="ana-izgara">
        <section className="kart">
          <h2>Başvurularım</h2>
          {liste === null && !hata && <p className="bos">Yükleniyor…</p>}

          {liste && liste.length === 0 && (
            <>
              <Bildirim tur="bilgi" mesaj="Henüz bir etik kurul başvurunuz bulunmuyor." />
              <button className="dugme" onClick={() => navigate('/basvurular?yeni=1')}>
                Yeni başvuru başlat
              </button>
            </>
          )}

          {liste && liste.length > 0 && (
            <>
              <dl className="sayilar">
                <div>
                  <dt>Devam eden</dt>
                  <dd>{taslaklar.length}</dd>
                </div>
                <div>
                  <dt>Gönderilen</dt>
                  <dd>{gonderilenler.length}</dd>
                </div>
              </dl>

              {sonTaslak && (
                <div className="son-taslak">
                  <p>
                    <strong>{sonTaslak.arastirmaAdi || 'Adsız taslak'}</strong>
                    <br />
                    <span className="meta">
                      Son kayıt {tarihSaat(sonTaslak.guncellemeTarihi)} · {sonTaslak.sonAdim}. adımda kaldınız
                    </span>
                  </p>
                  <Link className="dugme" to={`/basvurular/${sonTaslak.id}`}>
                    Kaldığım yerden devam et
                  </Link>
                </div>
              )}

              {gonderilenler.length > 0 && (
                <ul className="kisa-liste">
                  {gonderilenler.slice(0, 3).map((b) => (
                    <li key={b.id}>
                      <Link to={`/basvurular/${b.id}`}>{b.arastirmaAdi}</Link>
                      <DurumKasesi durum={b.durum} />
                    </li>
                  ))}
                </ul>
              )}

              <div className="dugmeler" style={{ marginTop: 16 }}>
                <Link className="dugme ikincil" to="/basvurular">
                  Bütün başvurularım
                </Link>
                <button className="dugme metin" onClick={() => navigate('/basvurular?yeni=1')}>
                  Yeni başvuru başlat
                </button>
              </div>
            </>
          )}
        </section>

        <section className="kart">
          <h2>Kişi bilgilerim</h2>
          <KisiBilgileri />
          <Link className="dugme ikincil kucuk" to="/profil" style={{ marginTop: 8 }}>
            Ayrıntıları görüntüle
          </Link>
        </section>
      </div>
    </>
  )
}

/** Kullanıcının bilgileri (ana sayfa ve profil sayfasında) */
export function KisiBilgileri({ tam = false }: { tam?: boolean }) {
  const { kullanici: k } = useOturum()
  if (!k) return null
  return (
    <dl className="bilgi-listesi">
      <dt>Ad soyad</dt>
      <dd>
        {k.ad} {k.soyad}
      </dd>
      <dt>Çalıştığı birim</dt>
      <dd>{k.calistigiBirim}</dd>
      <dt>Kurumsal e-posta</dt>
      <dd>{k.kurumsalEmail}</dd>
      {tam && (
        <>
          <dt>E-posta (giriş)</dt>
          <dd>{k.email}</dd>
          <dt>Telefon</dt>
          <dd>{k.telefon || '-'}</dd>
          <dt>Adres</dt>
          <dd>{k.adres || '-'}</dd>
          <dt>Kayıt tarihi</dt>
          <dd>{tarihSaat(k.kayitTarihi)}</dd>
        </>
      )}
      <dt>Yetkiler</dt>
      <dd>{k.roller.map((r) => ROL_ADLARI[r]).join(', ')}</dd>
    </dl>
  )
}
