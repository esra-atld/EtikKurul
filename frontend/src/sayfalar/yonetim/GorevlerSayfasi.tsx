// Yetkililerin iş listesi. Hangi sekmelerin görüneceği kullanıcının rolüne bağlı:
//   Arşiv Kayıt  : Kayıt bekleyenler, Arşiv
//   Yetkili Birim: İşlem bekleyenler, Etik kurulda olanlar
//   Etik Kurul   : Oyumu bekleyenler, Oy verdiklerim

import { useEffect, useState, type FormEvent } from 'react'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import { DURUM_ADLARI, tarihSaat } from '../../api/etiketler'
import { hataMesaji } from '../../api/istemci'
import { yonetimApi } from '../../api/servisler'
import type { GorevKutusu, Rol, YonetimOzet } from '../../api/tipler'
import { Bildirim, DurumKasesi, GeriBaglantisi, SonucYazisi, sonuctanKabul } from '../../bilesenler/Ortak'
import { useOturum } from '../../oturum/OturumBaglami'

interface Sekme {
  kutu: GorevKutusu
  ad: string
  rol: Rol
  bosMesaj: string
}

export const SEKMELER: Sekme[] = [
  { kutu: 'arsiv-gelen', ad: 'Kayıt bekleyenler', rol: 'EvrakKayit', bosMesaj: 'Arşiv kaydı bekleyen başvuru yok.' },
  { kutu: 'arsiv', ad: 'Arşiv', rol: 'EvrakKayit', bosMesaj: 'Arşivde kayıt bulunamadı.' },
  { kutu: 'birim', ad: 'İşlem bekleyenler', rol: 'Dekanlik', bosMesaj: 'Yetkili birimde işlem bekleyen başvuru yok.' },
  { kutu: 'birim-izlenen', ad: 'Etik kurulda olanlar', rol: 'Dekanlik', bosMesaj: 'Etik kurulda oylamada olan başvuru yok.' },
  { kutu: 'oy-bekleyen', ad: 'Oyumu bekleyenler', rol: 'EtikKurul', bosMesaj: 'Oyunuzu bekleyen başvuru yok.' },
  { kutu: 'oy-verilen', ad: 'Oy verdiklerim', rol: 'EtikKurul', bosMesaj: 'Henüz oy verdiğiniz bir başvuru yok.' },
]

/** Kullanıcının görebileceği sekmeler. Yönetici, etik kurul oyu dışındaki bütün sekmeleri görür. */
export function sekmelerim(roller: Rol[]): Sekme[] {
  const admin = roller.includes('Admin')
  return SEKMELER.filter((s) => roller.includes(s.rol) || (admin && s.rol !== 'EtikKurul'))
}

export function GorevlerSayfasi() {
  const { kullanici } = useOturum()
  const [parametreler, setParametreler] = useSearchParams()
  const [liste, setListe] = useState<YonetimOzet[] | null>(null)
  const [hata, setHata] = useState<string | null>(null)
  const [arama, setArama] = useState('')

  const sekmeler = kullanici ? sekmelerim(kullanici.roller) : []
  const aktif = sekmeler.find((s) => s.kutu === parametreler.get('kutu')) ?? sekmeler[0]

  function yukle(a = arama) {
    if (!aktif) return
    setListe(null)
    setHata(null)
    yonetimApi
      .liste(aktif.kutu, aktif.kutu === 'arsiv' ? a : undefined)
      .then(setListe)
      .catch((e) => {
        setListe([])
        setHata(hataMesaji(e).mesaj)
      })
  }

  useEffect(() => {
    setArama('')
    yukle('')
  }, [aktif?.kutu])

  if (!kullanici) return null
  if (sekmeler.length === 0) return <Navigate to="/" replace />

  function ara(e: FormEvent) {
    e.preventDefault()
    yukle()
  }

  return (
    <>
      <GeriBaglantisi to="/" metin="Ana sayfa" />
      <div className="sayfa-basligi">
        <div>
          <h1>Görevlerim</h1>
          <p>Size gelen başvuruları açıp inceleyin ve işlemi yapın.</p>
        </div>
      </div>

      <div className="sekmeler" role="tablist">
        {sekmeler.map((s) => (
          <button
            key={s.kutu}
            role="tab"
            aria-selected={s.kutu === aktif?.kutu}
            className={s.kutu === aktif?.kutu ? 'aktif' : ''}
            onClick={() => setParametreler({ kutu: s.kutu })}
          >
            {s.ad}
          </button>
        ))}
      </div>

      <div className="kart">
        {aktif?.kutu === 'arsiv' && (
          <form onSubmit={ara} className="dugmeler" style={{ marginBottom: 16 }}>
            <label className="gizli-baslik" htmlFor="arsiv-arama">
              Arşivde ara
            </label>
            <input
              id="arsiv-arama"
              type="text"
              placeholder="Kayıt sayısı, araştırma adı veya başvuran"
              value={arama}
              onChange={(e) => setArama(e.target.value)}
              style={{ maxWidth: 380 }}
            />
            <button className="dugme ikincil">Ara</button>
          </form>
        )}

        {hata && <Bildirim tur="hata" mesaj={hata} />}
        {liste === null && <p className="bos">Yükleniyor…</p>}
        {liste?.length === 0 && !hata && <Bildirim tur="bilgi" mesaj={aktif?.bosMesaj ?? 'Kayıt yok.'} />}

        {liste && liste.length > 0 && (
          <div className="tablo-kap">
            <table className="tablo yigin">
              <thead>
                <tr>
                  <th>Başvuru</th>
                  <th>Başvuran</th>
                  <th>Arşiv kaydı</th>
                  <th>Durum</th>
                  <th>
                    <span className="gizli-baslik">İşlem</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {liste.map((b) => (
                  <tr key={b.id}>
                    <td>
                      <strong>{b.arastirmaAdi}</strong>
                      <br />
                      <span className="meta">
                        No {b.id} · Gönderildi {tarihSaat(b.gonderimTarihi)}
                      </span>
                    </td>
                    <td data-baslik="Başvuran">
                      {b.basvuranAdSoyad}
                      <br />
                      <span className="meta">{b.basvuranBirim}</span>
                    </td>
                    <td data-baslik="Arşiv kaydı">
                      {b.evrakKayitNo ? (
                        <>
                          {b.evrakKayitNo}
                          <br />
                          <span className="meta">{tarihSaat(b.evrakKayitTarihi)}</span>
                        </>
                      ) : (
                        <span className="meta">Kaydedilmedi</span>
                      )}
                    </td>
                    <td data-baslik="Durum">
                      <DurumKasesi durum={b.durum} kabul={sonuctanKabul(b.sonuc)} />
                      {b.sonuc && <div style={{ marginTop: 6 }}><SonucYazisi sonuc={b.sonuc} /></div>}
                      {b.benimOyum !== null && (
                        <div className="meta" style={{ marginTop: 6 }}>
                          Oyum: {b.benimOyum ? 'Onaylıyorum' : 'Onaylamıyorum'}
                        </div>
                      )}
                    </td>
                    <td>
                      <Link className="dugme ikincil kucuk" to={`/gorevler/${b.id}`} title={DURUM_ADLARI[b.durum]}>
                        Aç
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </>
  )
}
