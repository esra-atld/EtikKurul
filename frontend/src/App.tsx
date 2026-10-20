import { Navigate, Route, Routes } from 'react-router-dom'
import { Duzen, GirisGerekli } from './bilesenler/Duzen'
import { AdminSayfasi } from './sayfalar/AdminSayfasi'
import { AnaSayfa } from './sayfalar/AnaSayfa'
import { BasvurularimSayfasi } from './sayfalar/BasvurularimSayfasi'
import { BasvuruSayfasi } from './sayfalar/basvuru/BasvuruSayfasi'
import { GirisSayfasi } from './sayfalar/GirisSayfasi'
import { KayitSayfasi } from './sayfalar/KayitSayfasi'
import { ProfilSayfasi } from './sayfalar/ProfilSayfasi'
import { GorevlerSayfasi } from './sayfalar/yonetim/GorevlerSayfasi'
import { YonetimBasvuruSayfasi } from './sayfalar/yonetim/YonetimBasvuruSayfasi'

// Sayfa adresleri
export default function App() {
  return (
    <Routes>
      <Route element={<Duzen />}>
        <Route path="/giris" element={<GirisSayfasi />} />
        <Route path="/kayit" element={<KayitSayfasi />} />

        {/* Buradakiler giriş gerektirir */}
        <Route element={<GirisGerekli />}>
          <Route path="/" element={<AnaSayfa />} />
          <Route path="/profil" element={<ProfilSayfasi />} />
          <Route path="/basvurular" element={<BasvurularimSayfasi />} />
          <Route path="/basvurular/:id" element={<BasvuruSayfasi />} />
          <Route path="/admin" element={<AdminSayfasi />} />
          <Route path="/gorevler" element={<GorevlerSayfasi />} />
          <Route path="/gorevler/:id" element={<YonetimBasvuruSayfasi />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
