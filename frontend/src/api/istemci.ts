// Backend ile konuşan tek yer. Bütün istekler buradan geçer:
//  - Token'ı otomatik ekler (Authorization: Bearer ...)
//  - Hata cevaplarını okunur bir mesaja çevirir
//  - 401 gelirse (token süresi doldu) oturumu kapatır

const TOKEN_ANAHTARI = 'etikkurul.token'

export function tokenAl(): string | null {
  try {
    return localStorage.getItem(TOKEN_ANAHTARI)
  } catch {
    return null
  }
}

export function tokenKaydet(token: string | null) {
  try {
    if (token) localStorage.setItem(TOKEN_ANAHTARI, token)
    else localStorage.removeItem(TOKEN_ANAHTARI)
  } catch {
    /* tarayıcı depolamayı engelliyorsa sessizce geç */
  }
}

/** Oturum düştüğünde çağrılacak fonksiyon (OturumBaglami ayarlar) */
let oturumDustu: () => void = () => {}
export function oturumDustugundeCagir(fn: () => void) {
  oturumDustu = fn
}

/** API'den gelen hata. Ekranda gösterilecek mesajı ve varsa ayrıntı listesini taşır. */
export class ApiHatasi extends Error {
  durumKodu: number
  ayrintilar: string[]
  govde: unknown

  constructor(durumKodu: number, mesaj: string, ayrintilar: string[] = [], govde: unknown = null) {
    super(mesaj)
    this.durumKodu = durumKodu
    this.ayrintilar = ayrintilar
    this.govde = govde
  }
}

/** Backend'in döndüğü farklı hata biçimlerini tek tipe çevirir. */
function hatayiCoz(durumKodu: number, govde: unknown): ApiHatasi {
  if (govde && typeof govde === 'object') {
    const g = govde as Record<string, unknown>

    // Bizim { mesaj, eksikler / eksikBelgeler } biçimimiz
    if (typeof g.mesaj === 'string') {
      const ayrintilar: string[] = []
      if (Array.isArray(g.eksikBelgeler)) ayrintilar.push(...(g.eksikBelgeler as string[]))
      if (Array.isArray(g.eksikler)) {
        for (const e of g.eksikler as { adim: number; mesaj: string }[]) ayrintilar.push(`${e.adim}. adım: ${e.mesaj}`)
      }
      return new ApiHatasi(durumKodu, g.mesaj, ayrintilar, govde)
    }

    // ASP.NET doğrulama hatası: { title, errors: { Alan: ["mesaj"] } }
    if (g.errors && typeof g.errors === 'object') {
      const ayrintilar = Object.values(g.errors as Record<string, string[]>).flat()
      return new ApiHatasi(durumKodu, 'Lütfen işaretli alanları düzeltin.', ayrintilar, govde)
    }
  }

  const varsayilan: Record<number, string> = {
    400: 'İstek hatalı.',
    401: 'Oturumunuzun süresi doldu, tekrar giriş yapın.',
    403: 'Bu işlem için yetkiniz yok.',
    404: 'Kayıt bulunamadı.',
    409: 'Bu işlem şu anda yapılamaz.',
    500: 'Sunucuda bir hata oluştu. Terminaldeki hata mesajına bakın.',
  }
  return new ApiHatasi(durumKodu, varsayilan[durumKodu] ?? `Beklenmeyen hata (${durumKodu}).`, [], govde)
}

type Secenekler = {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  govde?: unknown // JSON olarak gönderilir
  form?: FormData // dosya yükleme için
}

async function istekGonder(yol: string, s: Secenekler = {}): Promise<Response> {
  const basliklar: Record<string, string> = {}
  const token = tokenAl()
  if (token) basliklar.Authorization = `Bearer ${token}`

  let body: BodyInit | undefined
  if (s.form) {
    body = s.form // Content-Type'ı tarayıcı kendisi koyar
  } else if (s.govde !== undefined) {
    basliklar['Content-Type'] = 'application/json'
    body = JSON.stringify(s.govde)
  }

  let cevap: Response
  try {
    cevap = await fetch(`/api${yol}`, { method: s.method ?? 'GET', headers: basliklar, body })
  } catch {
    throw new ApiHatasi(0, 'Sunucuya ulaşılamadı. Backend çalışıyor mu? (dotnet run)')
  }

  if (cevap.status === 401 && token) oturumDustu()

  if (!cevap.ok) {
    let govde: unknown = null
    try {
      govde = await cevap.json()
    } catch {
      /* gövde yok veya JSON değil */
    }
    throw hatayiCoz(cevap.status, govde)
  }
  return cevap
}

/** JSON dönen istek */
export async function api<T>(yol: string, s: Secenekler = {}): Promise<T> {
  const cevap = await istekGonder(yol, s)
  if (cevap.status === 204) return undefined as T
  const metin = await cevap.text()
  return (metin ? JSON.parse(metin) : undefined) as T
}

/** Dosya (PDF) dönen istek: yeni sekmede açar */
export async function dosyaAc(yol: string) {
  // Sekmeyi hemen açıyoruz, yoksa tarayıcı açılır pencere engelleyicisine takılır
  const sekme = window.open('', '_blank')
  try {
    const cevap = await istekGonder(yol)
    const blob = await cevap.blob()
    const adres = URL.createObjectURL(blob)
    if (sekme) sekme.location.href = adres
    else window.location.href = adres
    setTimeout(() => URL.revokeObjectURL(adres), 60_000)
  } catch (e) {
    sekme?.close()
    throw e
  }
}

/** Herhangi bir hatayı ekranda gösterilecek metne çevirir */
export function hataMesaji(e: unknown): { mesaj: string; ayrintilar: string[] } {
  if (e instanceof ApiHatasi) return { mesaj: e.message, ayrintilar: e.ayrintilar }
  if (e instanceof Error) return { mesaj: e.message, ayrintilar: [] }
  return { mesaj: 'Bilinmeyen bir hata oluştu.', ayrintilar: [] }
}
