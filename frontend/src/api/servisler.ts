// Backend'deki her uç noktanın (endpoint) bir fonksiyon karşılığı.
// Sayfalar fetch kullanmaz, buradaki fonksiyonları çağırır.

import { api, dosyaAc } from './istemci'
import type {
  BasvuruDetay,
  BasvuruFormu,
  BasvuruOzet,
  BasvuruTuru,
  EksikAlan,
  GorevKutusu,
  GerekliEk,
  GirisCevabi,
  KayitIstegi,
  Kullanici,
  Rol,
  Secenek,
  Takip,
  YonetimDetay,
  YonetimOzet,
  YuklenenEk,
} from './tipler'

// ───── Giriş / kayıt ─────
export const authApi = {
  giris: (email: string, sifre: string) =>
    api<GirisCevabi>('/auth/giris', { method: 'POST', govde: { email, sifre } }),
  kayit: (istek: KayitIstegi) => api<{ mesaj: string }>('/auth/kayit', { method: 'POST', govde: istek }),
  ben: () => api<Kullanici>('/auth/ben'),
}

// ───── Sabit listeler ─────
export const tanimApi = {
  secenekler: () => api<Secenek[]>('/tanimlar/secenekler'),
}

// ───── Başvurular ─────
export const basvuruApi = {
  benimkiler: () => api<BasvuruOzet[]>('/basvurular'),
  olustur: (arastirmaAdi: string, basvuruTuru: BasvuruTuru = 'YeniBasvuru') =>
    api<BasvuruDetay>('/basvurular', { method: 'POST', govde: { arastirmaAdi, basvuruTuru } }),
  getir: (id: number) => api<BasvuruDetay>(`/basvurular/${id}`),
  kaydet: (id: number, form: BasvuruFormu) =>
    api<BasvuruDetay>(`/basvurular/${id}`, { method: 'PUT', govde: form }),
  sil: (id: number) => api<void>(`/basvurular/${id}`, { method: 'DELETE' }),
  gerekliEkler: (id: number) => api<GerekliEk[]>(`/basvurular/${id}/gerekli-ekler`),
  takip: (id: number) => api<Takip>(`/basvurular/${id}/takip`),

  eksikler: (id: number) => api<{ tamamMi: boolean; eksikler: EksikAlan[] }>(`/basvurular/${id}/eksikler`),
  kilitle: (id: number) => api<{ mesaj: string }>(`/basvurular/${id}/kilitle`, { method: 'POST' }),
  kilidiAc: (id: number) => api<{ mesaj: string }>(`/basvurular/${id}/kilidi-ac`, { method: 'POST' }),
  pdfAc: (id: number) => dosyaAc(`/basvurular/${id}/pdf`),
  gonder: (id: number) => api<{ mesaj: string }>(`/basvurular/${id}/gonder`, { method: 'POST' }),

  ekler: (id: number) => api<YuklenenEk[]>(`/basvurular/${id}/ekler`),
  ekYukle: (id: number, ekTuruId: number, dosya: File, arastirmaciId?: number) => {
    const form = new FormData()
    form.append('EkTuruId', String(ekTuruId))
    if (arastirmaciId) form.append('BasvuruArastirmacisiId', String(arastirmaciId))
    form.append('Dosya', dosya)
    return api<{ mesaj: string }>(`/basvurular/${id}/ekler`, { method: 'POST', form })
  },
  ekAc: (id: number, ekId: number) => dosyaAc(`/basvurular/${id}/ekler/${ekId}/dosya`),
  ekSil: (id: number, ekId: number) => api<void>(`/basvurular/${id}/ekler/${ekId}`, { method: 'DELETE' }),
}

// ───── Admin ─────
export const adminApi = {
  kullanicilar: (arama?: string) =>
    api<Kullanici[]>(`/admin/kullanicilar${arama ? `?arama=${encodeURIComponent(arama)}` : ''}`),
  rolleriGuncelle: (id: string, roller: Rol[]) =>
    api<Kullanici>(`/admin/kullanicilar/${id}/roller`, { method: 'PUT', govde: { roller } }),
  aktiflik: (id: string, aktifMi: boolean) =>
    api<Kullanici>(`/admin/kullanicilar/${id}/aktiflik`, { method: 'PUT', govde: { aktifMi } }),
}

// ───── Yetkililer: Arşiv Kayıt, Yetkili Birim, Etik Kurul ─────
export const yonetimApi = {
  liste: (kutu: GorevKutusu, arama?: string) =>
    api<YonetimOzet[]>(`/yonetim/basvurular?kutu=${kutu}${arama ? `&arama=${encodeURIComponent(arama)}` : ''}`),
  detay: (id: number) => api<YonetimDetay>(`/yonetim/basvurular/${id}`),
  pdfAc: (id: number) => dosyaAc(`/yonetim/basvurular/${id}/pdf`),
  ekAc: (id: number, ekId: number) => dosyaAc(`/yonetim/basvurular/${id}/ekler/${ekId}/dosya`),
  onerilenKayitNo: () => api<{ kayitNo: string }>('/yonetim/onerilen-kayit-no'),
  arsiveKaydet: (id: number, kayitNo: string, kayitTarihi: string) =>
    api<{ mesaj: string }>(`/yonetim/basvurular/${id}/arsive-kaydet`, { method: 'POST', govde: { kayitNo, kayitTarihi } }),
  etikKurulaGonder: (id: number) =>
    api<{ mesaj: string }>(`/yonetim/basvurular/${id}/etik-kurula-gonder`, { method: 'POST' }),
  oyVer: (id: number, onayliyorum: boolean) =>
    api<{ mesaj: string }>(`/yonetim/basvurular/${id}/oy`, { method: 'POST', govde: { onayliyorum } }),
  basvuranaIlet: (id: number) =>
    api<{ mesaj: string }>(`/yonetim/basvurular/${id}/basvurana-ilet`, { method: 'POST' }),
}
