// Giriş yapmış kullanıcının bilgisi bütün sayfalarda buradan okunur.
// Token tarayıcıda saklanır; sayfa yenilense bile oturum açık kalır.

import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { oturumDustugundeCagir, tokenAl, tokenKaydet } from '../api/istemci'
import { authApi } from '../api/servisler'
import type { Kullanici, Rol } from '../api/tipler'

interface OturumDegeri {
  kullanici: Kullanici | null
  yukleniyor: boolean
  girisYap: (email: string, sifre: string) => Promise<void>
  cikisYap: () => void
  rolVarMi: (rol: Rol) => boolean
}

const OturumBaglami = createContext<OturumDegeri | null>(null)

export function OturumSaglayici({ children }: { children: ReactNode }) {
  const [kullanici, setKullanici] = useState<Kullanici | null>(null)
  const [yukleniyor, setYukleniyor] = useState(true)

  const cikisYap = useCallback(() => {
    tokenKaydet(null)
    setKullanici(null)
  }, [])

  // Sayfa açılınca: kayıtlı token varsa kim olduğumuzu sor
  useEffect(() => {
    oturumDustugundeCagir(cikisYap)
    if (!tokenAl()) {
      setYukleniyor(false)
      return
    }
    authApi
      .ben()
      .then(setKullanici)
      .catch(() => tokenKaydet(null))
      .finally(() => setYukleniyor(false))
  }, [cikisYap])

  const girisYap = useCallback(async (email: string, sifre: string) => {
    const cevap = await authApi.giris(email, sifre)
    tokenKaydet(cevap.token)
    setKullanici(cevap.kullanici)
  }, [])

  const deger = useMemo<OturumDegeri>(
    () => ({
      kullanici,
      yukleniyor,
      girisYap,
      cikisYap,
      rolVarMi: (rol) => kullanici?.roller.includes(rol) ?? false,
    }),
    [kullanici, yukleniyor, girisYap, cikisYap],
  )

  return <OturumBaglami.Provider value={deger}>{children}</OturumBaglami.Provider>
}

export function useOturum(): OturumDegeri {
  const d = useContext(OturumBaglami)
  if (!d) throw new Error('useOturum, OturumSaglayici içinde kullanılmalı')
  return d
}
