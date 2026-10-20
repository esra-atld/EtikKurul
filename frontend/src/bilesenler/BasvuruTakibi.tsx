// "Başvurum nerede?": başvurunun geçtiği aşamalar (zaman çizelgesi) ve varsa etik kurul kararı.
// Hem başvuran hem yetkililer kullanır; yetkililer işlemi yapan kişiyi de görür.

import { DURUM_ADLARI, tarihSaat } from '../api/etiketler'
import type { Hareket, KararOzet } from '../api/tipler'

export function KararKutusu({ karar }: { karar: KararOzet }) {
  const kabul = karar.kararTuru === 'Uygun'
  return (
    <div className={`karar-kutusu ${kabul ? 'kabul' : 'red'}`}>
      <span className={`kase buyuk ${kabul ? 'sonuc' : 'iade'}`}>{kabul ? 'Kabul edildi' : 'Reddedildi'}</span>
      <div>
        <strong>Etik kurul kararı</strong>
        <p>{karar.ozet}</p>
        <span className="meta">Karar tarihi: {tarihSaat(karar.kararTarihi)}</span>
      </div>
    </div>
  )
}

export function BasvuruTakibi({
  hareketler,
  evrakKayitNo,
  evrakKayitTarihi,
}: {
  hareketler: Hareket[]
  evrakKayitNo: string | null
  evrakKayitTarihi: string | null
}) {
  return (
    <>
      {evrakKayitNo && (
        <dl className="bilgi-listesi">
          <dt>Arşiv kayıt sayısı</dt>
          <dd>{evrakKayitNo}</dd>
          <dt>Arşiv kayıt tarihi</dt>
          <dd>{tarihSaat(evrakKayitTarihi)}</dd>
        </dl>
      )}
      <ol className="zaman-cizelgesi">
        {[...hareketler].reverse().map((h, i) => (
          <li key={`${h.tarih}-${i}`} className={i === 0 ? 'son' : ''}>
            <span className="zaman">{tarihSaat(h.tarih)}</span>
            <strong>{DURUM_ADLARI[h.yeniDurum]}</strong>
            {h.aciklama && <p>{h.aciklama}</p>}
            {h.islemYapan && <span className="meta">İşlemi yapan: {h.islemYapan}</span>}
          </li>
        ))}
      </ol>
    </>
  )
}
