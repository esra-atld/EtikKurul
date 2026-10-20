// Her sayfanın üstündeki lacivert bant ve menü.

import { Link, NavLink, Navigate, Outlet, useNavigate } from 'react-router-dom'
import { useOturum } from '../oturum/OturumBaglami'
import { sekmelerim } from '../sayfalar/yonetim/GorevlerSayfasi'

export function Duzen() {
  const { kullanici, cikisYap, rolVarMi } = useOturum()
  const navigate = useNavigate()

  function cikis() {
    cikisYap()
    navigate('/giris', { replace: true })
  }

  return (
    <>
      <header className="ust-bant">
        <div className="ust-bant-ic">
          <Link to="/" className="kurum">
            <strong>Etik Kurul Başvuru Sistemi</strong>
            <span>Selçuk Üniversitesi Teknoloji Fakültesi</span>
          </Link>

          {kullanici && (
            <nav className="ust-menu" aria-label="Ana menü">
              <NavLink to="/" end>
                Ana sayfa
              </NavLink>
              <NavLink to="/basvurular">Başvurularım</NavLink>
              {sekmelerim(kullanici.roller).length > 0 && <NavLink to="/gorevler">Görevlerim</NavLink>}
              {rolVarMi('Admin') && <NavLink to="/admin">Kullanıcılar ve yetkiler</NavLink>}
            </nav>
          )}

          {kullanici && (
            <div className="kullanici-kutusu">
              <NavLink to="/profil" className="profil-baglantisi" title="Kişi bilgilerim">
                {kullanici.ad} {kullanici.soyad}
              </NavLink>
              <button className="dugme ikincil kucuk" onClick={cikis}>
                Çıkış yap
              </button>
            </div>
          )}
        </div>
      </header>
      <main className="sayfa">
        <Outlet />
      </main>
    </>
  )
}

/** Giriş yapmamış kullanıcıyı giriş sayfasına gönderir */
export function GirisGerekli() {
  const { kullanici, yukleniyor } = useOturum()

  if (yukleniyor) return <p className="bos">Yükleniyor…</p>
  if (!kullanici) return <Navigate to="/giris" replace />
  return <Outlet />
}
