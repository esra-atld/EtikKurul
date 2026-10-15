import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { hataMesaji } from '../api/istemci'
import { authApi } from '../api/servisler'
import type { KayitIstegi } from '../api/tipler'
import { Alan, Bildirim, TelefonKutusu, telefonHatasi } from '../bilesenler/Ortak'

const BOS: KayitIstegi = {
  ad: '',
  soyad: '',
  calistigiBirim: '',
  email: '',
  kurumsalEmail: '',
  telefon: '',
  adres: '',
  sifre: '',
}

export function KayitSayfasi() {
  const navigate = useNavigate()
  const [form, setForm] = useState<KayitIstegi>(BOS)
  const [hata, setHata] = useState<{ mesaj: string; ayrintilar: string[] } | null>(null)
  const [gonderiliyor, setGonderiliyor] = useState(false)

  const degistir = (alan: keyof KayitIstegi) => (e: { target: { value: string } }) =>
    setForm((f) => ({ ...f, [alan]: e.target.value }))

  async function gonder(e: FormEvent) {
    e.preventDefault()
    const telHata = telefonHatasi(form.telefon)
    if (telHata) {
      setHata({ mesaj: telHata, ayrintilar: [] })
      return
    }
    setHata(null)
    setGonderiliyor(true)
    try {
      await authApi.kayit(form)
      navigate('/giris', { state: { kayitBasarili: true } })
    } catch (err) {
      setHata(hataMesaji(err))
    } finally {
      setGonderiliyor(false)
    }
  }

  return (
    <div className="kart" style={{ maxWidth: 760, margin: '24px auto' }}>
      <h1>Kayıt ol</h1>
      <p style={{ color: 'var(--soluk)' }}>
        Kayıt olduktan sonra etik kurul başvurusu yapabilirsiniz. Yetkili rolleri (evrak kayıt, dekanlık, etik kurul)
        sistem yöneticisi verir.
      </p>

      {hata && <Bildirim tur="hata" mesaj={hata.mesaj} ayrintilar={hata.ayrintilar} />}

      <form onSubmit={gonder}>
        <div className="izgara">
          <Alan etiket="Ad">
            <input type="text" value={form.ad} onChange={degistir('ad')} required autoComplete="given-name" />
          </Alan>
          <Alan etiket="Soyad">
            <input type="text" value={form.soyad} onChange={degistir('soyad')} required autoComplete="family-name" />
          </Alan>
        </div>
        <Alan etiket="Çalıştığı birim" ipucu="Örneğin: Bilgisayar Mühendisliği Bölümü">
          <input type="text" value={form.calistigiBirim} onChange={degistir('calistigiBirim')} required />
        </Alan>
        <div className="izgara">
          <Alan etiket="E-posta" ipucu="Giriş yaparken bunu kullanacaksınız">
            <input type="email" value={form.email} onChange={degistir('email')} required autoComplete="email" />
          </Alan>
          <Alan etiket="Kurumsal e-posta" ipucu="selcuk.edu.tr ile bitmeli">
            <input type="email" value={form.kurumsalEmail} onChange={degistir('kurumsalEmail')} required />
          </Alan>
        </div>
        <div className="izgara">
          <Alan
            etiket="Telefon"
            ipucu="0 ile başlayan 11 hane"
            uyarilar={form.telefon.length === 11 ? [telefonHatasi(form.telefon)].filter((x): x is string => !!x) : []}
          >
            <TelefonKutusu deger={form.telefon} degisti={(t) => setForm((f) => ({ ...f, telefon: t }))} zorunlu />
          </Alan>
          <Alan etiket="Şifre" ipucu="En az 8 karakter; büyük harf, küçük harf ve rakam">
            <input
              type="password"
              value={form.sifre}
              onChange={degistir('sifre')}
              required
              autoComplete="new-password"
            />
          </Alan>
        </div>
        <Alan etiket="Adres" ipucu="Resmi yazışmalar bu adrese yapılır">
          <textarea value={form.adres} onChange={degistir('adres')} required style={{ minHeight: 70 }} />
        </Alan>

        <div className="dugmeler">
          <button className="dugme" disabled={gonderiliyor}>
            {gonderiliyor ? 'Kaydediliyor…' : 'Kayıt ol'}
          </button>
          <Link to="/giris">Zaten hesabım var</Link>
        </div>
      </form>
    </div>
  )
}
