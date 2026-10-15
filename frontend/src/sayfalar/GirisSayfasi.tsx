import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { hataMesaji } from '../api/istemci'
import { Alan, Bildirim } from '../bilesenler/Ortak'
import { useOturum } from '../oturum/OturumBaglami'

export function GirisSayfasi() {
  const { kullanici, girisYap } = useOturum()
  const navigate = useNavigate()
  const konum = useLocation()
  const durum = (konum.state as { kayitBasarili?: boolean } | null) ?? {}

  const [email, setEmail] = useState('')
  const [sifre, setSifre] = useState('')
  const [hata, setHata] = useState<string | null>(null)
  const [gonderiliyor, setGonderiliyor] = useState(false)

  // Giriş yapınca her zaman ana sayfa açılır
  if (kullanici) return <Navigate to="/" replace />

  async function gonder(e: FormEvent) {
    e.preventDefault()
    setHata(null)
    setGonderiliyor(true)
    try {
      await girisYap(email, sifre)
      navigate('/', { replace: true })
    } catch (err) {
      setHata(hataMesaji(err).mesaj)
    } finally {
      setGonderiliyor(false)
    }
  }

  return (
    <div className="dar">
      <div className="kart">
        <h1>Giriş yap</h1>
        <p style={{ color: 'var(--soluk)' }}>Etik kurul başvurunuzu başlatmak veya takip etmek için giriş yapın.</p>

        {durum.kayitBasarili && <Bildirim tur="basari" mesaj="Kaydınız oluşturuldu. Şimdi giriş yapabilirsiniz." />}
        {hata && <Bildirim tur="hata" mesaj={hata} />}

        <form onSubmit={gonder}>
          <Alan etiket="E-posta">
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="email" />
          </Alan>
          <Alan etiket="Şifre">
            <input
              type="password"
              value={sifre}
              onChange={(e) => setSifre(e.target.value)}
              required
              autoComplete="current-password"
            />
          </Alan>
          <div className="dugmeler">
            <button className="dugme" disabled={gonderiliyor}>
              {gonderiliyor ? 'Giriş yapılıyor…' : 'Giriş yap'}
            </button>
            <Link to="/kayit">Hesabım yok, kayıt olacağım</Link>
          </div>
        </form>
      </div>
    </div>
  )
}
