import { useEffect, useState, type FormEvent } from 'react'
import { Navigate } from 'react-router-dom'
import { ROL_ADLARI, tarihSaat } from '../api/etiketler'
import { hataMesaji } from '../api/istemci'
import { adminApi } from '../api/servisler'
import type { Kullanici, Rol } from '../api/tipler'
import { Bildirim, GeriBaglantisi } from '../bilesenler/Ortak'
import { useOturum } from '../oturum/OturumBaglami'

const ROLLER: Rol[] = ['Basvuran', 'EvrakKayit', 'Dekanlik', 'EtikKurul', 'Admin']

/** Yetkili kişilerin tanımlandığı sayfa (sadece Admin) */
export function AdminSayfasi() {
  const { rolVarMi, kullanici: ben } = useOturum()
  const [liste, setListe] = useState<Kullanici[] | null>(null)
  const [arama, setArama] = useState('')
  const [mesaj, setMesaj] = useState<{ tur: 'hata' | 'basari'; metin: string } | null>(null)

  function yukle(a = arama) {
    adminApi
      .kullanicilar(a)
      .then(setListe)
      .catch((e) => setMesaj({ tur: 'hata', metin: hataMesaji(e).mesaj }))
  }
  useEffect(() => yukle(''), [])

  if (!rolVarMi('Admin')) return <Navigate to="/basvurular" replace />

  function guncelle(yeni: Kullanici) {
    setListe((l) => l?.map((k) => (k.id === yeni.id ? yeni : k)) ?? null)
  }

  async function rolDegistir(k: Kullanici, rol: Rol, secili: boolean) {
    const roller = secili ? [...k.roller, rol] : k.roller.filter((r) => r !== rol)
    try {
      guncelle(await adminApi.rolleriGuncelle(k.id, roller))
      setMesaj({ tur: 'basari', metin: `${k.ad} ${k.soyad}: roller güncellendi.` })
    } catch (e) {
      setMesaj({ tur: 'hata', metin: hataMesaji(e).mesaj })
    }
  }

  async function aktiflikDegistir(k: Kullanici) {
    try {
      guncelle(await adminApi.aktiflik(k.id, !k.aktifMi))
      setMesaj({ tur: 'basari', metin: `${k.ad} ${k.soyad} ${k.aktifMi ? 'pasif yapıldı' : 'aktif yapıldı'}.` })
    } catch (e) {
      setMesaj({ tur: 'hata', metin: hataMesaji(e).mesaj })
    }
  }

  function ara(e: FormEvent) {
    e.preventDefault()
    yukle()
  }

  return (
    <>
      <GeriBaglantisi to="/" metin="Ana sayfa" />
      <div className="sayfa-basligi">
        <div>
          <h1>Kullanıcılar ve yetkiler</h1>
          <p>Evrak kayıt, dekanlık ve etik kurul görevlilerini buradan tanımlayın.</p>
        </div>
        <form onSubmit={ara} className="dugmeler">
          <label className="gizli-baslik" htmlFor="arama">
            Kullanıcı ara
          </label>
          <input
            id="arama"
            type="text"
            placeholder="Ad, soyad veya e-posta"
            value={arama}
            onChange={(e) => setArama(e.target.value)}
            style={{ width: 240 }}
          />
          <button className="dugme ikincil">Ara</button>
        </form>
      </div>

      {mesaj && <Bildirim tur={mesaj.tur} mesaj={mesaj.metin} />}

      <div className="kart tablo-kap">
        {liste === null ? (
          <p className="bos">Yükleniyor…</p>
        ) : (
          <table className="tablo">
            <thead>
              <tr>
                <th>Kullanıcı</th>
                <th>Birim</th>
                <th>Roller</th>
                <th>Durum</th>
              </tr>
            </thead>
            <tbody>
              {liste.map((k) => (
                <tr key={k.id}>
                  <td>
                    <strong>
                      {k.ad} {k.soyad}
                    </strong>
                    <br />
                    <span style={{ color: 'var(--soluk)' }}>{k.email}</span>
                    <br />
                    <span style={{ color: 'var(--soluk)', fontSize: '0.85rem' }}>Kayıt: {tarihSaat(k.kayitTarihi)}</span>
                  </td>
                  <td>{k.calistigiBirim}</td>
                  <td>
                    <div className="roller">
                      {ROLLER.map((r) => (
                        <label key={r}>
                          <input
                            type="checkbox"
                            checked={k.roller.includes(r)}
                            disabled={r === 'Admin' && k.id === ben?.id}
                            onChange={(e) => rolDegistir(k, r, e.target.checked)}
                          />
                          {ROL_ADLARI[r]}
                        </label>
                      ))}
                    </div>
                  </td>
                  <td>
                    <button
                      className={`dugme kucuk ${k.aktifMi ? 'ikincil' : 'tehlike'}`}
                      onClick={() => aktiflikDegistir(k)}
                      disabled={k.id === ben?.id}
                      title={k.aktifMi ? 'Pasif yapılan kullanıcı giriş yapamaz' : ''}
                    >
                      {k.aktifMi ? 'Aktif' : 'Pasif'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </>
  )
}
