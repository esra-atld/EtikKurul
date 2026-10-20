// Backend'deki kod adlarının (YuksekLisansTezi gibi) ekranda görünecek Türkçe karşılıkları.

import type {
  ArastirmaciRolu,
  ArastirmaNiteligi,
  BasvuruDurumu,
  BasvuruTuru,
  DestekDurumu,
  DestekKurumTuru,
  GiderTuru,
  LiteraturTuru,
  Rol,
} from './tipler'

export const ROL_ADLARI: Record<Rol, string> = {
  Basvuran: 'Başvuran',
  EvrakKayit: 'Arşiv Kayıt',
  Dekanlik: 'Yetkili Birim',
  EtikKurul: 'Etik Kurul Üyesi',
  Admin: 'Yönetici',
}

export const DURUM_ADLARI: Record<BasvuruDurumu, string> = {
  Taslak: 'Taslak',
  ImzaliBelgeBekleniyor: 'İmzalı belgeler bekleniyor',
  EvrakKayitta: 'Arşiv kayıtta',
  IadeEdildi: 'İade edildi',
  Dekanlikta: 'Yetkili birimde',
  EtikKurulda: 'Etik kurulda (oylamada)',
  KararDekanlikta: 'Karar yetkili birimde',
  Sonuclandi: 'Sonuçlandı',
  Arsivlendi: 'Arşivlendi',
}

/** Durum kaşesinin rengi */
export function durumSinifi(d: BasvuruDurumu): string {
  switch (d) {
    case 'Taslak':
      return 'taslak'
    case 'ImzaliBelgeBekleniyor':
      return 'bekliyor'
    case 'IadeEdildi':
      return 'iade'
    case 'Sonuclandi':
    case 'Arsivlendi':
      return 'sonuc'
    default:
      return 'yolda'
  }
}

export const BASVURU_TURU_ADLARI: Record<BasvuruTuru, string> = {
  YeniBasvuru: 'Yeni başvuru',
  TekrarBasvuruRevizyon: 'Tekrar başvuru (revizyon)',
  ProtokolEkipDegisikligi: 'Protokol veya ekip değişikliği',
  OnaylanmisProjeDevami: 'Daha önce onaylanmış bir projenin devamı',
}

export const NITELIK_ADLARI: Record<ArastirmaNiteligi, string> = {
  BilimselArastirmaProjesiBap: 'Bilimsel Araştırma Projesi (BAP)',
  YuksekLisansTezi: 'Yüksek lisans tez projesi',
  DoktoraSanattaYeterlilikTezi: 'Doktora / sanatta yeterlilik tez projesi',
  TezsizYuksekLisansProjesi: 'Tezsiz yüksek lisans projesi',
  Diger: 'Diğer',
}

export const ARASTIRMACI_ROL_ADLARI: Record<ArastirmaciRolu, string> = {
  SorumluArastirmaci: 'Sorumlu araştırmacı (proje yürütücüsü / danışman)',
  IkinciDanismanTezYazari: '2. danışman / tez yazarı',
  EkipUyesi: 'Ekip üyesi',
}

export const GIDER_ADLARI: Record<GiderTuru, string> = {
  HizmetAlimi: 'Hizmet alımı',
  DemirbasAlimi: 'Demirbaş alımı',
  SarfMalzemeAlimi: 'Sarf malzeme alımı',
  Diger: 'Diğer',
}

export const DESTEK_DURUM_ADLARI: Record<DestekDurumu, string> = {
  Destekleyen: 'Destekliyor',
  DesteklemesiPlanlanan: 'Desteklemesi planlanıyor',
}

export const DESTEK_KURUM_ADLARI: Record<DestekKurumTuru, string> = {
  Universite: 'Üniversite',
  Tubitak: 'TÜBİTAK',
  Dpt: 'DPT',
  KalkinmaAjansi: 'Kalkınma Ajansı',
  Uluslararasi: 'Uluslararası',
  Diger: 'Diğer',
}

export const LITERATUR_ADLARI: Record<LiteraturTuru, string> = {
  SecilmisLiteratur: 'Seçilmiş literatür',
  YontemLiteraturu: 'Yöntemde yararlanılan literatür',
}

export function tarihSaat(iso: string | null): string {
  if (!iso) return '-'
  return new Date(iso).toLocaleString('tr-TR', { dateStyle: 'medium', timeStyle: 'short' })
}

export function para(tutar: number): string {
  return tutar.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' TL'
}
