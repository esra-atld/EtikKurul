// Backend'deki DTO'ların TypeScript karşılıkları.
// Backend'de bir alan değişirse burası da güncellenmeli.

export type Rol = 'Basvuran' | 'EvrakKayit' | 'Dekanlik' | 'EtikKurul' | 'Admin'

export type BasvuruDurumu =
  | 'Taslak'
  | 'ImzaliBelgeBekleniyor'
  | 'EvrakKayitta'
  | 'IadeEdildi'
  | 'Dekanlikta'
  | 'EtikKurulda'
  | 'KararDekanlikta'
  | 'Sonuclandi'
  | 'Arsivlendi'

export type BasvuruTuru =
  | 'YeniBasvuru'
  | 'TekrarBasvuruRevizyon'
  | 'ProtokolEkipDegisikligi'
  | 'OnaylanmisProjeDevami'

export type ArastirmaNiteligi =
  | 'BilimselArastirmaProjesiBap'
  | 'YuksekLisansTezi'
  | 'DoktoraSanattaYeterlilikTezi'
  | 'TezsizYuksekLisansProjesi'
  | 'Diger'

export type ArastirmaciRolu = 'SorumluArastirmaci' | 'IkinciDanismanTezYazari' | 'EkipUyesi'
export type GiderTuru = 'HizmetAlimi' | 'DemirbasAlimi' | 'SarfMalzemeAlimi' | 'Diger'
export type DestekDurumu = 'Destekleyen' | 'DesteklemesiPlanlanan'
export type DestekKurumTuru = 'Universite' | 'Tubitak' | 'Dpt' | 'KalkinmaAjansi' | 'Uluslararasi' | 'Diger'
export type LiteraturTuru = 'SecilmisLiteratur' | 'YontemLiteraturu'
export type SecenekGrubu = 'VeriToplamaAraci' | 'KatilimciGrubu' | 'Uygulama'

// ───── Kullanıcı ─────

export interface Kullanici {
  id: string
  ad: string
  soyad: string
  email: string
  kurumsalEmail: string
  telefon: string | null
  calistigiBirim: string
  adres: string
  aktifMi: boolean
  kayitTarihi: string
  roller: Rol[]
}

export interface GirisCevabi {
  token: string
  bitisZamani: string
  kullanici: Kullanici
}

export interface KayitIstegi {
  ad: string
  soyad: string
  calistigiBirim: string
  email: string
  kurumsalEmail: string
  telefon: string
  adres: string
  sifre: string
}

// ───── Başvuru ─────

export interface Arastirmaci {
  id: number // yeni satırda 0
  rol: ArastirmaciRolu
  unvan: string | null
  adSoyad: string
  telefon: string | null
  email: string | null
  gorevYeriAdres: string | null
}

export interface DestekKurumu {
  durum: DestekDurumu
  kurumTuru: DestekKurumTuru
  aciklama: string | null
}

export interface GiderKalemi {
  tur: GiderTuru
  aciklama: string
  tutar: number
}

export interface IsPaketi {
  no: number
  ad: string
  kimler: string | null
  sure: string | null
}

export interface Risk {
  isPaketiNo: number
  riskAciklamasi: string
  bPlani: string | null
}

export interface Literatur {
  tur: LiteraturTuru
  kaynak: string
}

/** Wizard'ın her kaydette gönderdiği form (PUT /api/basvurular/{id}) */
export interface BasvuruFormu {
  sonAdim: number
  deneyVeyaBiyolojikMateryalVarMi: boolean | null
  arastirmaAdi: string | null
  basvuruTuru: BasvuruTuru
  arastirmaNiteligi: ArastirmaNiteligi | null
  arastirmaNiteligiDiger: string | null
  bagliKurum: string | null
  ustBasvuruId: number | null
  veriToplamaAraciDiger: string | null
  veriToplamaBaslangic: string | null // "2026-11-01"
  veriToplamaBitis: string | null
  tahminiSureAy: number | null
  veriAraciBaskasinaAitMi: boolean | null
  turkceOzet: string | null
  anahtarKelimeler: string | null
  yabanciDilOzet: string | null
  keywords: string | null
  amac: string | null
  konuKapsamLiteratur: string | null
  materyalYontem: string | null
  ozgunDeger: string | null
  yayginEtki: string | null
  yanlisBilgiVeyaGizlilikGerekiyorMu: boolean | null
  yanlisBilgiVeyaGizlilikAciklama: string | null
  saglikTehdidiVarMi: boolean | null
  saglikTehdidiAciklama: string | null
  kisilikHaklariKorunuyorMu: boolean | null
  kisilikHaklariAciklama: string | null
  resitOlmayanKatilimciVarMi: boolean | null
  resitOlmayanKatilimciAciklama: string | null
  yeterliBilgiVeriliyorMu: boolean | null
  yeterliBilgiAciklama: string | null
  gonulluKatilimOnlemleri: string | null
  katilmaCikmaKosullari: string | null
  beklenenKatilimciSayisi: number | null
  katilimciGrubuDiger: string | null
  uygulamaDiger: string | null
  secenekIdleri: number[]
  arastirmacilar: Arastirmaci[]
  veriToplamaYerleri: { ad: string }[]
  destekKurumlari: DestekKurumu[]
  giderKalemleri: GiderKalemi[]
  isPaketleri: IsPaketi[]
  riskler: Risk[]
  literaturKaynaklari: Literatur[]
}

export interface BasvuruDetay extends BasvuruFormu {
  id: number
  durum: BasvuruDurumu
  formKilitliMi: boolean
  duzenlenebilirMi: boolean
  evrakKayitNo: string | null
  evrakKayitTarihi: string | null
  toplamGider: number
  olusturmaTarihi: string
  guncellemeTarihi: string
  gonderimTarihi: string | null
}

export interface BasvuruOzet {
  id: number
  arastirmaAdi: string
  basvuruTuru: BasvuruTuru
  durum: BasvuruDurumu
  sonAdim: number
  evrakKayitNo: string | null
  olusturmaTarihi: string
  guncellemeTarihi: string
  gonderimTarihi: string | null
  /** Etik kurul oylaması bittiyse kararın özeti (ör. "Oy birliği ile kabul edildi (3 kabul, 0 red)") */
  sonuc: string | null
}

// ───── Başvuru takibi ve yetkili işlemleri ─────

export interface Hareket {
  tarih: string
  oncekiDurum: BasvuruDurumu | null
  yeniDurum: BasvuruDurumu
  aciklama: string | null
  islemYapan: string | null
}

export interface KararOzet {
  kararTuru: 'Uygun' | 'UygunDegil' | 'DuzeltmeIstenir'
  oyBirligiMi: boolean
  kabulOySayisi: number
  redOySayisi: number
  kararTarihi: string
  ozet: string
}

export interface Takip {
  durum: BasvuruDurumu
  evrakKayitNo: string | null
  evrakKayitTarihi: string | null
  hareketler: Hareket[]
  karar: KararOzet | null
}

export type GorevKutusu = 'arsiv-gelen' | 'arsiv' | 'birim' | 'birim-izlenen' | 'oy-bekleyen' | 'oy-verilen'

export interface YonetimOzet {
  id: number
  arastirmaAdi: string
  basvuranAdSoyad: string
  basvuranBirim: string
  durum: BasvuruDurumu
  evrakKayitNo: string | null
  evrakKayitTarihi: string | null
  gonderimTarihi: string | null
  sonuc: string | null
  benimOyum: boolean | null
}

export type YonetimIslemi = 'ArsiveKaydet' | 'EtikKuruluGonder' | 'OyVer' | 'BasvuranaIlet'

export interface YonetimDetay {
  basvuru: BasvuruDetay
  basvuran: { adSoyad: string; calistigiBirim: string; email: string; kurumsalEmail: string; telefon: string | null }
  hareketler: Hareket[]
  ekler: YuklenenEk[]
  oylama: { uyeSayisi: number; oyVerenSayisi: number; benimOyum: boolean | null; benimOyumBekleniyor: boolean } | null
  karar: KararOzet | null
  islemler: YonetimIslemi[]
}

// ───── Tanımlar ─────

export interface Secenek {
  id: number
  grup: SecenekGrubu
  ad: string
  sira: number
  aciklamaGerekirMi: boolean
  cocukRizasiGerektirirMi: boolean
}

// ───── Belgeler ─────

export interface GerekliEk {
  ekTuruId: number
  kod: string
  ad: string
  aciklama: string | null
  zorunluMu: boolean
  neden: string
  gerekenAdet: number
  yuklenenAdet: number
  tamamMi: boolean
  sistemUretirMi: boolean
  cokluDosyaMi: boolean
}

export interface YuklenenEk {
  id: number
  ekTuruId: number
  ekTuruKod: string
  ekTuruAd: string
  basvuruArastirmacisiId: number | null
  orijinalDosyaAdi: string
  boyutByte: number
  yuklemeTarihi: string
}

export interface EksikAlan {
  adim: number
  alan: string
  mesaj: string
}
