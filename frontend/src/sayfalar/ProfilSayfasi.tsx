import { GeriBaglantisi } from '../bilesenler/Ortak'
import { KisiBilgileri } from './AnaSayfa'

/** Kişi bilgilerim */
export function ProfilSayfasi() {
  return (
    <>
      <GeriBaglantisi to="/" metin="Ana sayfa" />
      <div className="sayfa-basligi">
        <div>
          <h1>Kişi bilgilerim</h1>
          <p>Kayıt olurken girdiğiniz bilgiler. Değişiklik için sistem yöneticisine başvurun.</p>
        </div>
      </div>
      <div className="kart" style={{ maxWidth: 720 }}>
        <KisiBilgileri tam />
      </div>
    </>
  )
}
