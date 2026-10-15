// Birçok sayfada kullanılan küçük parçalar.

import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { DURUM_ADLARI, durumSinifi } from '../api/etiketler'
import type { BasvuruDurumu } from '../api/tipler'

/** Hata / başarı / bilgi kutusu */
export function Bildirim({
  tur,
  mesaj,
  ayrintilar = [],
}: {
  tur: 'hata' | 'basari' | 'bilgi'
  mesaj: string
  ayrintilar?: string[]
}) {
  return (
    <div className={`bildirim ${tur}`} role={tur === 'hata' ? 'alert' : 'status'}>
      {mesaj}
      {ayrintilar.length > 0 && (
        <ul>
          {ayrintilar.map((a) => (
            <li key={a}>{a}</li>
          ))}
        </ul>
      )}
    </div>
  )
}

/** Başvurunun durumunu evrak kaşesi gibi gösterir */
export function DurumKasesi({ durum, buyuk = false }: { durum: BasvuruDurumu; buyuk?: boolean }) {
  return <span className={`kase ${durumSinifi(durum)}${buyuk ? ' buyuk' : ''}`}>{DURUM_ADLARI[durum]}</span>
}

/** Alanın hemen altında çıkan kırmızı uyarılar */
export function AlanUyarilari({ uyarilar }: { uyarilar?: string[] }) {
  if (!uyarilar || uyarilar.length === 0) return null
  return (
    <span className="alan-uyarisi" role="alert">
      {uyarilar.map((u) => (
        <span key={u}>{u}</span>
      ))}
    </span>
  )
}

/** Etiketli form alanı. uyarilar verilirse alanın altında kırmızı gösterilir. */
export function Alan({
  etiket,
  ipucu,
  uyarilar,
  children,
}: {
  etiket: string
  ipucu?: string
  uyarilar?: string[]
  children: ReactNode
}) {
  const uyariVar = !!uyarilar && uyarilar.length > 0
  return (
    <label className={`alan${uyariVar ? ' hatali' : ''}`}>
      <span className="etiket">{etiket}</span>
      {ipucu && <span className="ipucu">{ipucu}</span>}
      {children}
      <AlanUyarilari uyarilar={uyarilar} />
    </label>
  )
}

/** Evet / Hayır seçimi (null = henüz cevaplanmadı) */
export function EvetHayir({
  deger,
  degisti,
  etiket,
}: {
  deger: boolean | null
  degisti: (d: boolean) => void
  etiket: string
}) {
  return (
    <div className="evet-hayir" role="group" aria-label={etiket}>
      <button type="button" aria-pressed={deger === true} onClick={() => degisti(true)}>
        Evet
      </button>
      <button type="button" aria-pressed={deger === false} onClick={() => degisti(false)}>
        Hayır
      </button>
    </div>
  )
}

// ───── Telefon ─────

/** Telefon: 0 ile başlayan 11 hane (ör. 05551234567 veya 03322232323) */
export function telefonHatasi(tel: string | null | undefined): string | null {
  if (!tel) return null
  if (!/^0\d{10}$/.test(tel)) return 'Telefon numarası 0 ile başlayan 11 haneli olmalı (ör. 05551234567).'
  return null
}

/** Sadece rakam kabul eden, en fazla 11 haneli telefon kutusu */
export function TelefonKutusu({
  deger,
  degisti,
  zorunlu = false,
}: {
  deger: string
  degisti: (s: string) => void
  zorunlu?: boolean
}) {
  return (
    <input
      type="tel"
      inputMode="numeric"
      autoComplete="tel"
      placeholder="05XXXXXXXXX"
      maxLength={11}
      required={zorunlu}
      value={deger}
      onChange={(e) => degisti(e.target.value.replace(/\D/g, '').slice(0, 11))}
    />
  )
}

/** Sayfanın üstündeki "geri" bağlantısı */
export function GeriBaglantisi({ to, metin }: { to: string; metin: string }) {
  return (
    <Link to={to} className="geri-baglantisi">
      ‹ {metin}
    </Link>
  )
}

/** Boş metni null'a çevirir (backend boş alanları null bekliyor) */
export function bosIseNull(s: string): string | null {
  return s.trim() === '' ? null : s
}

/** Sayı kutusundaki değeri sayıya çevirir */
export function sayiVeyaNull(s: string): number | null {
  if (s.trim() === '') return null
  const n = Number(s)
  return Number.isFinite(n) ? n : null
}

/** Bugünün tarihi "2026-10-15" biçiminde (tarih kutularının en küçük değeri için) */
export function bugun(): string {
  const d = new Date()
  const ay = String(d.getMonth() + 1).padStart(2, '0')
  const gun = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${ay}-${gun}`
}
