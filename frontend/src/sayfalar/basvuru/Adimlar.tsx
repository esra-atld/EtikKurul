// Başvuru formunun 1–7. adımları. Her adım formun bir bölümünü düzenler.
// Bölüm adları ve sorular "TF Etik Kurul Başvuru Formu"ndan alınmıştır.
//
// UYARILAR: Eksik/hatalı alanların uyarıları ilgili alanın hemen altında gösterilir.
//  - uyari('AlanAdi'): backend'in eksik kontrolünden gelen uyarılar (alan adları
//    backend'deki BasvuruDogrulayici ile aynı)
//  - telefon ve tarih gibi kurallar ayrıca yazarken anında kontrol edilir

import { useEffect, useState } from 'react'
import {
  ARASTIRMACI_ROL_ADLARI,
  BASVURU_TURU_ADLARI,
  DESTEK_DURUM_ADLARI,
  DESTEK_KURUM_ADLARI,
  GIDER_ADLARI,
  NITELIK_ADLARI,
  para,
} from '../../api/etiketler'
import { basvuruApi } from '../../api/servisler'
import type {
  Arastirmaci,
  ArastirmaciRolu,
  ArastirmaNiteligi,
  BasvuruFormu,
  BasvuruOzet,
  BasvuruTuru,
  DestekDurumu,
  DestekKurumTuru,
  GiderTuru,
  LiteraturTuru,
  Secenek,
  SecenekGrubu,
} from '../../api/tipler'
import {
  Alan,
  AlanUyarilari,
  Bildirim,
  EvetHayir,
  TelefonKutusu,
  bosIseNull,
  bugun,
  sayiVeyaNull,
  telefonHatasi,
} from '../../bilesenler/Ortak'

export interface AdimProps {
  form: BasvuruFormu
  guncelle: (parca: Partial<BasvuruFormu>) => void
  /** Bu alan için gösterilecek uyarılar */
  uyari: (alan: string) => string[]
  /** Form düzenlenebiliyor mu (kilitli/gönderilmiş başvuruda anlık kontroller kapalı) */
  duzenlenebilir: boolean
}

// ═══════════════════════ Yardımcılar ═══════════════════════

/** Bir listedeki tek satırı değiştirip yeni listeyi döner */
function satirDegistir<T>(liste: T[], i: number, parca: Partial<T>): T[] {
  return liste.map((s, j) => (j === i ? { ...s, ...parca } : s))
}

/** null olmayan uyarıları tek listede toplar */
function birlestir(...parcalar: (string | null | undefined | string[])[]): string[] {
  const sonuc: string[] = []
  for (const p of parcalar) {
    if (Array.isArray(p)) sonuc.push(...p)
    else if (p) sonuc.push(p)
  }
  return [...new Set(sonuc)]
}

function SatirBasligi({ baslik, sil }: { baslik: string; sil: () => void }) {
  return (
    <div className="satir-ust">
      <strong>{baslik}</strong>
      <button type="button" className="dugme metin kucuk" onClick={sil}>
        Kaldır
      </button>
    </div>
  )
}

/** Belli bir gruptaki seçenekleri onay kutusu olarak gösterir */
function CokluSecim({
  grup,
  secenekler,
  form,
  guncelle,
}: Pick<AdimProps, 'form' | 'guncelle'> & { grup: SecenekGrubu; secenekler: Secenek[] }) {
  const gruptakiler = secenekler.filter((s) => s.grup === grup)
  function degistir(id: number, secili: boolean) {
    const yeni = secili ? [...form.secenekIdleri, id] : form.secenekIdleri.filter((x) => x !== id)
    guncelle({ secenekIdleri: yeni })
  }
  return (
    <div className="secenekler">
      {gruptakiler.map((s) => (
        <label key={s.id}>
          <input
            type="checkbox"
            checked={form.secenekIdleri.includes(s.id)}
            onChange={(e) => degistir(s.id, e.target.checked)}
          />
          {s.ad}
        </label>
      ))}
    </div>
  )
}

/** "Diğer" seçildiyse açıklama kutusu gerekiyor mu? */
function digerSecildi(grup: SecenekGrubu, secenekler: Secenek[], idler: number[]) {
  return secenekler.some((s) => s.grup === grup && s.aciklamaGerekirMi && idler.includes(s.id))
}

/** Adımın başında, o adımdaki uyarı sayısını gösteren kutu */
function AdimOzeti({ sayi }: { sayi: number }) {
  if (sayi === 0) return null
  return (
    <Bildirim
      tur="hata"
      mesaj={`Bu adımda tamamlanması gereken ${sayi} alan var. Kırmızı uyarılı alanları kontrol edin.`}
    />
  )
}

// ═══════════════════════ 1. Genel bilgiler ═══════════════════════

const ADIM1_ALANLARI = ['DeneyVeyaBiyolojikMateryalVarMi', 'ArastirmaAdi', 'ArastirmaNiteligi', 'ArastirmaNiteligiDiger', 'BagliKurum', 'UstBasvuruId']

export function Adim1Genel({ form, guncelle, uyari }: AdimProps) {
  const [oncekiler, setOncekiler] = useState<BasvuruOzet[]>([])
  const yeniDegil = form.basvuruTuru !== 'YeniBasvuru'

  useEffect(() => {
    if (yeniDegil) basvuruApi.benimkiler().then(setOncekiler).catch(() => setOncekiler([]))
  }, [yeniDegil])

  return (
    <>
      <h2>Genel bilgiler</h2>
      <AdimOzeti sayi={ADIM1_ALANLARI.flatMap(uyari).length} />

      <div className="soru">
        <span className="etiket">
          Çalışma insan veya hayvan üzerinde deney niteliği taşıyor ya da biyolojik materyal (kan, idrar, doku örneği
          vb.) kullanıyor mu?
        </span>
        <EvetHayir
          etiket="Deney veya biyolojik materyal"
          deger={form.deneyVeyaBiyolojikMateryalVarMi}
          degisti={(d) => guncelle({ deneyVeyaBiyolojikMateryalVarMi: d })}
        />
        <AlanUyarilari uyarilar={uyari('DeneyVeyaBiyolojikMateryalVarMi')} />
      </div>

      <div className="soru">
        <Alan
          etiket="Araştırmanın adı"
          ipucu="Proje başvurusundaki konu başlığıyla aynı olmalıdır."
          uyarilar={uyari('ArastirmaAdi')}
        >
          <input
            type="text"
            value={form.arastirmaAdi ?? ''}
            onChange={(e) => guncelle({ arastirmaAdi: bosIseNull(e.target.value) })}
          />
        </Alan>

        <div className="izgara">
          <Alan etiket="Etik kurula başvuru durumu">
            <select
              value={form.basvuruTuru}
              onChange={(e) => guncelle({ basvuruTuru: e.target.value as BasvuruTuru, ustBasvuruId: null })}
            >
              {Object.entries(BASVURU_TURU_ADLARI).map(([k, v]) => (
                <option key={k} value={k}>
                  {v}
                </option>
              ))}
            </select>
          </Alan>
          <Alan etiket="Araştırmanın niteliği" uyarilar={uyari('ArastirmaNiteligi')}>
            <select
              value={form.arastirmaNiteligi ?? ''}
              onChange={(e) =>
                guncelle({ arastirmaNiteligi: (e.target.value || null) as ArastirmaNiteligi | null })
              }
            >
              <option value="">Seçin</option>
              {Object.entries(NITELIK_ADLARI).map(([k, v]) => (
                <option key={k} value={k}>
                  {v}
                </option>
              ))}
            </select>
          </Alan>
        </div>

        {yeniDegil && (
          <Alan
            etiket="Önceki başvuru"
            ipucu="Revizyonu veya devamı olan başvurunuzu seçin."
            uyarilar={uyari('UstBasvuruId')}
          >
            <select
              value={form.ustBasvuruId ?? ''}
              onChange={(e) => guncelle({ ustBasvuruId: sayiVeyaNull(e.target.value) })}
            >
              <option value="">Seçin</option>
              {oncekiler.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.id} · {b.arastirmaAdi || 'Adsız'}
                </option>
              ))}
            </select>
          </Alan>
        )}

        {form.arastirmaNiteligi === 'Diger' && (
          <Alan etiket="Niteliği belirtin" uyarilar={uyari('ArastirmaNiteligiDiger')}>
            <input
              type="text"
              value={form.arastirmaNiteligiDiger ?? ''}
              onChange={(e) => guncelle({ arastirmaNiteligiDiger: bosIseNull(e.target.value) })}
            />
          </Alan>
        )}

        <Alan
          etiket="Sorumlu araştırmacının bağlı olduğu kurum / kuruluş"
          ipucu="Proje önerisindeki kurum ya da sorumlu araştırmacının kurumu"
          uyarilar={uyari('BagliKurum')}
        >
          <input
            type="text"
            value={form.bagliKurum ?? ''}
            onChange={(e) => guncelle({ bagliKurum: bosIseNull(e.target.value) })}
          />
        </Alan>
      </div>
    </>
  )
}

// ═══════════════════════ 2. Araştırmacılar ═══════════════════════

/** Unvan seçenekleri. Listede olmayan bir unvan için "Diğer" seçilip yazılır. */
const UNVANLAR = [
  'Prof. Dr.',
  'Doç. Dr.',
  'Dr. Öğr. Üyesi',
  'Öğr. Gör. Dr.',
  'Öğr. Gör.',
  'Arş. Gör. Dr.',
  'Arş. Gör.',
  'Doktora Öğrencisi',
  'Yüksek Lisans Öğrencisi',
  'Lisans Öğrencisi',
]

function UnvanSecimi({ deger, degisti }: { deger: string | null; degisti: (u: string | null) => void }) {
  const [digerSecili, setDigerSecili] = useState(!!deger && !UNVANLAR.includes(deger))
  const seciliDeger = digerSecili ? 'Diğer' : (deger ?? '')

  return (
    <>
      <select
        value={seciliDeger}
        onChange={(e) => {
          const v = e.target.value
          if (v === 'Diğer') {
            setDigerSecili(true)
            degisti(null)
          } else {
            setDigerSecili(false)
            degisti(v || null)
          }
        }}
      >
        <option value="">Seçin</option>
        {UNVANLAR.map((u) => (
          <option key={u} value={u}>
            {u}
          </option>
        ))}
        <option value="Diğer">Diğer</option>
      </select>
      {digerSecili && (
        <input
          type="text"
          style={{ marginTop: 6 }}
          placeholder="Unvanı yazın"
          aria-label="Diğer unvan"
          value={deger ?? ''}
          onChange={(e) => degisti(bosIseNull(e.target.value))}
        />
      )}
    </>
  )
}

const BOS_ARASTIRMACI: Arastirmaci = {
  id: 0,
  rol: 'EkipUyesi',
  unvan: null,
  adSoyad: '',
  telefon: null,
  email: null,
  gorevYeriAdres: null,
}

export function Adim2Arastirmacilar({ form, guncelle, uyari, duzenlenebilir }: AdimProps) {
  const liste = form.arastirmacilar
  const tezMi =
    form.arastirmaNiteligi === 'YuksekLisansTezi' ||
    form.arastirmaNiteligi === 'DoktoraSanattaYeterlilikTezi' ||
    form.arastirmaNiteligi === 'TezsizYuksekLisansProjesi'

  // İlk araştırmacı eklenirken rolü "sorumlu araştırmacı" olarak gelsin
  const ekle = () =>
    guncelle({
      arastirmacilar: [
        ...liste,
        { ...BOS_ARASTIRMACI, rol: liste.some((a) => a.rol === 'SorumluArastirmaci') ? 'EkipUyesi' : 'SorumluArastirmaci' },
      ],
    })

  const genelUyarilar = uyari('Arastirmacilar')

  return (
    <>
      <h2>Araştırmacılar</h2>
      <p style={{ color: 'var(--soluk)' }}>
        Bir sorumlu araştırmacı (proje yürütücüsü veya tez danışmanı) zorunludur.
        {tezMi && ' Tez çalışmalarında tez yazarını da ekleyin; taahhütnameyi ikisi birlikte imzalar.'}
      </p>
      {genelUyarilar.length > 0 && <Bildirim tur="hata" mesaj="Bu adımda tamamlanması gerekenler:" ayrintilar={genelUyarilar} />}

      {liste.length === 0 && <p className="bos">Henüz araştırmacı eklenmedi.</p>}

      {liste.map((a, i) => {
        const telUyari = duzenlenebilir ? telefonHatasi(a.telefon) : null
        return (
          <div className="satir" key={a.id || `yeni-${i}`}>
            <SatirBasligi
              baslik={a.adSoyad || `${i + 1}. araştırmacı`}
              sil={() => guncelle({ arastirmacilar: liste.filter((_, j) => j !== i) })}
            />
            <div className="izgara-3">
              <Alan etiket="Görevi">
                <select
                  value={a.rol}
                  onChange={(e) =>
                    guncelle({ arastirmacilar: satirDegistir(liste, i, { rol: e.target.value as ArastirmaciRolu }) })
                  }
                >
                  {Object.entries(ARASTIRMACI_ROL_ADLARI).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </Alan>
              <Alan etiket="Unvan">
                <UnvanSecimi
                  deger={a.unvan}
                  degisti={(u) => guncelle({ arastirmacilar: satirDegistir(liste, i, { unvan: u }) })}
                />
              </Alan>
              <Alan etiket="Ad soyad" uyarilar={duzenlenebilir && !a.adSoyad.trim() ? ['Ad soyad boş olamaz.'] : []}>
                <input
                  type="text"
                  value={a.adSoyad}
                  required
                  onChange={(e) => guncelle({ arastirmacilar: satirDegistir(liste, i, { adSoyad: e.target.value }) })}
                />
              </Alan>
            </div>
            <div className="izgara-3">
              <Alan etiket="Telefon (iş / cep)" ipucu="0 ile başlayan 11 hane" uyarilar={birlestir(telUyari)}>
                <TelefonKutusu
                  deger={a.telefon ?? ''}
                  degisti={(t) => guncelle({ arastirmacilar: satirDegistir(liste, i, { telefon: bosIseNull(t) }) })}
                />
              </Alan>
              <Alan etiket="E-posta">
                <input
                  type="email"
                  value={a.email ?? ''}
                  onChange={(e) =>
                    guncelle({ arastirmacilar: satirDegistir(liste, i, { email: bosIseNull(e.target.value) }) })
                  }
                />
              </Alan>
              <Alan etiket="Görev yeri / kurum adresi">
                <input
                  type="text"
                  value={a.gorevYeriAdres ?? ''}
                  onChange={(e) =>
                    guncelle({ arastirmacilar: satirDegistir(liste, i, { gorevYeriAdres: bosIseNull(e.target.value) }) })
                  }
                />
              </Alan>
            </div>
          </div>
        )
      })}

      <button type="button" className="dugme ikincil" onClick={ekle}>
        Araştırmacı ekle
      </button>
    </>
  )
}

// ═══════════════════════ 3. Veri toplama ═══════════════════════

const ADIM3_ALANLARI = ['VeriToplamaAraclari', 'VeriToplamaAraciDiger', 'VeriToplamaBaslangic', 'VeriToplamaBitis', 'TahminiSureAy', 'VeriToplamaYerleri', 'VeriAraciBaskasinaAitMi']

export function Adim3VeriToplama({ form, guncelle, uyari, duzenlenebilir, secenekler }: AdimProps & { secenekler: Secenek[] }) {
  const yerler = form.veriToplamaYerleri
  const bugunTarih = bugun()

  // Tarih kuralları (yazarken anında)
  const baslangicUyari =
    duzenlenebilir && form.veriToplamaBaslangic && form.veriToplamaBaslangic < bugunTarih
      ? 'Başlangıç tarihi bugünden önce olamaz.'
      : null
  const bitisUyari =
    duzenlenebilir && form.veriToplamaBaslangic && form.veriToplamaBitis && form.veriToplamaBitis < form.veriToplamaBaslangic
      ? 'Bitiş tarihi başlangıç tarihinden önce olamaz.'
      : null

  return (
    <>
      <h2>Veri toplama</h2>
      <AdimOzeti sayi={ADIM3_ALANLARI.flatMap(uyari).length} />

      <div className="soru">
        <span className="etiket">Veri toplama aracının türü</span>
        <CokluSecim grup="VeriToplamaAraci" secenekler={secenekler} form={form} guncelle={guncelle} />
        <AlanUyarilari uyarilar={uyari('VeriToplamaAraclari')} />
        {digerSecildi('VeriToplamaAraci', secenekler, form.secenekIdleri) && (
          <div style={{ marginTop: 12 }}>
            <Alan etiket="Diğer veri toplama aracını belirtin" uyarilar={uyari('VeriToplamaAraciDiger')}>
              <input
                type="text"
                value={form.veriToplamaAraciDiger ?? ''}
                onChange={(e) => guncelle({ veriToplamaAraciDiger: bosIseNull(e.target.value) })}
              />
            </Alan>
          </div>
        )}
      </div>

      <div className="soru">
        <span className="etiket">Veri toplanması planlanan dönem</span>
        <div className="izgara-3">
          <Alan etiket="Başlangıç" uyarilar={birlestir(baslangicUyari, uyari('VeriToplamaBaslangic'))}>
            <input
              type="date"
              min={bugunTarih}
              value={form.veriToplamaBaslangic ?? ''}
              onChange={(e) => guncelle({ veriToplamaBaslangic: bosIseNull(e.target.value) })}
            />
          </Alan>
          <Alan etiket="Bitiş" uyarilar={birlestir(bitisUyari, uyari('VeriToplamaBitis'))}>
            <input
              type="date"
              min={form.veriToplamaBaslangic ?? bugunTarih}
              value={form.veriToplamaBitis ?? ''}
              onChange={(e) => guncelle({ veriToplamaBitis: bosIseNull(e.target.value) })}
            />
          </Alan>
          <Alan etiket="Tahmini araştırma süresi (ay)" uyarilar={uyari('TahminiSureAy')}>
            <input
              type="number"
              min={1}
              value={form.tahminiSureAy ?? ''}
              onChange={(e) => guncelle({ tahminiSureAy: sayiVeyaNull(e.target.value) })}
            />
          </Alan>
        </div>
      </div>

      <div className="soru">
        <span className="etiket">Veri toplanması planlanan yerler, kurum ve kuruluşlar</span>
        {yerler.map((y, i) => (
          <div className="dugmeler" key={i} style={{ marginBottom: 8, flexWrap: 'nowrap' }}>
            <input
              type="text"
              aria-label={`${i + 1}. yer`}
              value={y.ad}
              onChange={(e) => guncelle({ veriToplamaYerleri: satirDegistir(yerler, i, { ad: e.target.value }) })}
            />
            <button
              type="button"
              className="dugme metin kucuk"
              onClick={() => guncelle({ veriToplamaYerleri: yerler.filter((_, j) => j !== i) })}
            >
              Kaldır
            </button>
          </div>
        ))}
        <AlanUyarilari uyarilar={uyari('VeriToplamaYerleri')} />
        <button
          type="button"
          className="dugme ikincil kucuk"
          onClick={() => guncelle({ veriToplamaYerleri: [...yerler, { ad: '' }] })}
        >
          Yer ekle
        </button>
      </div>

      <div className="soru">
        <span className="etiket">Kullanacağınız anket, ölçek gibi veri toplama aracı başkasına mı ait?</span>
        <span className="ipucu" style={{ display: 'block', color: 'var(--soluk)', marginBottom: 8 }}>
          Evet ise, araç sahibinden aldığınız izni (Ek-11) yüklemeniz gerekecek.
        </span>
        <EvetHayir
          etiket="Veri aracı başkasına ait"
          deger={form.veriAraciBaskasinaAitMi}
          degisti={(d) => guncelle({ veriAraciBaskasinaAitMi: d })}
        />
        <AlanUyarilari uyarilar={uyari('VeriAraciBaskasinaAitMi')} />
      </div>
    </>
  )
}

// ═══════════════════════ 4. Destek ve giderler ═══════════════════════

export function Adim4DestekGider({ form, guncelle, uyari }: AdimProps) {
  const destekler = form.destekKurumlari
  const giderler = form.giderKalemleri
  const toplam = giderler.reduce((t, g) => t + (Number(g.tutar) || 0), 0)
  const destekUyarilari = uyari('DestekKurumlari')

  return (
    <>
      <h2>Destek ve giderler</h2>

      <div className="soru">
        <span className="etiket">Araştırmayı destekleyen veya desteklemesi planlanan kurumlar</span>
        <span className="ipucu" style={{ display: 'block', color: 'var(--soluk)', marginBottom: 8 }}>
          Destek yoksa boş bırakın (formda "Yoktur" yazar). Destekleyen kurum varsa Ek-4 protokolü istenir.
        </span>
        {destekUyarilari.length > 0 && <Bildirim tur="hata" mesaj="Tamamlanması gerekenler:" ayrintilar={destekUyarilari} />}
        {destekler.map((d, i) => (
          <div className="satir" key={i}>
            <SatirBasligi
              baslik={DESTEK_KURUM_ADLARI[d.kurumTuru]}
              sil={() => guncelle({ destekKurumlari: destekler.filter((_, j) => j !== i) })}
            />
            <div className="izgara-3">
              <Alan etiket="Durum">
                <select
                  value={d.durum}
                  onChange={(e) =>
                    guncelle({ destekKurumlari: satirDegistir(destekler, i, { durum: e.target.value as DestekDurumu }) })
                  }
                >
                  {Object.entries(DESTEK_DURUM_ADLARI).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </Alan>
              <Alan etiket="Kurum türü">
                <select
                  value={d.kurumTuru}
                  onChange={(e) =>
                    guncelle({
                      destekKurumlari: satirDegistir(destekler, i, { kurumTuru: e.target.value as DestekKurumTuru }),
                    })
                  }
                >
                  {Object.entries(DESTEK_KURUM_ADLARI).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </Alan>
              <Alan etiket="Açıklama">
                <input
                  type="text"
                  value={d.aciklama ?? ''}
                  placeholder="Kurumun adı"
                  onChange={(e) =>
                    guncelle({ destekKurumlari: satirDegistir(destekler, i, { aciklama: bosIseNull(e.target.value) }) })
                  }
                />
              </Alan>
            </div>
          </div>
        ))}
        <button
          type="button"
          className="dugme ikincil kucuk"
          onClick={() =>
            guncelle({ destekKurumlari: [...destekler, { durum: 'Destekleyen', kurumTuru: 'Universite', aciklama: null }] })
          }
        >
          Destek kurumu ekle
        </button>
      </div>

      <div className="soru">
        <span className="etiket">Araştırma giderleri</span>
        {giderler.map((g, i) => (
          <div className="satir" key={i}>
            <SatirBasligi baslik={GIDER_ADLARI[g.tur]} sil={() => guncelle({ giderKalemleri: giderler.filter((_, j) => j !== i) })} />
            <div className="izgara-3">
              <Alan etiket="Gider türü">
                <select
                  value={g.tur}
                  onChange={(e) =>
                    guncelle({ giderKalemleri: satirDegistir(giderler, i, { tur: e.target.value as GiderTuru }) })
                  }
                >
                  {Object.entries(GIDER_ADLARI).map(([k, v]) => (
                    <option key={k} value={k}>
                      {v}
                    </option>
                  ))}
                </select>
              </Alan>
              <Alan etiket="Açıklama">
                <input
                  type="text"
                  value={g.aciklama}
                  onChange={(e) => guncelle({ giderKalemleri: satirDegistir(giderler, i, { aciklama: e.target.value }) })}
                />
              </Alan>
              <Alan etiket="Tutar (TL)">
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={g.tutar}
                  onChange={(e) =>
                    guncelle({ giderKalemleri: satirDegistir(giderler, i, { tutar: sayiVeyaNull(e.target.value) ?? 0 }) })
                  }
                />
              </Alan>
            </div>
          </div>
        ))}
        {giderler.length > 0 && <p className="toplam">Toplam: {para(toplam)}</p>}
        <button
          type="button"
          className="dugme ikincil kucuk"
          onClick={() => guncelle({ giderKalemleri: [...giderler, { tur: 'SarfMalzemeAlimi', aciklama: '', tutar: 0 }] })}
        >
          Gider ekle
        </button>
      </div>
    </>
  )
}

// ═══════════════════════ 5. Proje içeriği ═══════════════════════

const ADIM5_ALANLARI = ['TurkceOzet', 'AnahtarKelimeler', 'YabanciDilOzet', 'Keywords', 'Amac', 'KonuKapsamLiteratur', 'MateryalYontem', 'OzgunDeger', 'YayginEtki']

function UzunMetin({
  etiket,
  ipucu,
  deger,
  degisti,
  uyarilar,
}: {
  etiket: string
  ipucu?: string
  deger: string | null
  degisti: (s: string | null) => void
  uyarilar?: string[]
}) {
  return (
    <Alan etiket={etiket} ipucu={ipucu} uyarilar={uyarilar}>
      <textarea value={deger ?? ''} onChange={(e) => degisti(bosIseNull(e.target.value))} />
    </Alan>
  )
}

function LiteraturListesi({
  tur,
  baslik,
  form,
  guncelle,
}: Pick<AdimProps, 'form' | 'guncelle'> & { tur: LiteraturTuru; baslik: string }) {
  const tum = form.literaturKaynaklari
  const buTur = tum.map((l, i) => ({ l, i })).filter((x) => x.l.tur === tur)
  return (
    <div style={{ marginBottom: 16 }}>
      <span className="etiket" style={{ display: 'block', fontWeight: 600, marginBottom: 6 }}>
        {baslik}
      </span>
      {buTur.map(({ l, i }, sira) => (
        <div className="dugmeler" key={i} style={{ marginBottom: 8, flexWrap: 'nowrap' }}>
          <span style={{ minWidth: 24 }}>{sira + 1}-</span>
          <input
            type="text"
            aria-label={`${baslik} ${sira + 1}`}
            value={l.kaynak}
            onChange={(e) => guncelle({ literaturKaynaklari: satirDegistir(tum, i, { kaynak: e.target.value }) })}
          />
          <button
            type="button"
            className="dugme metin kucuk"
            onClick={() => guncelle({ literaturKaynaklari: tum.filter((_, j) => j !== i) })}
          >
            Kaldır
          </button>
        </div>
      ))}
      <button
        type="button"
        className="dugme ikincil kucuk"
        onClick={() => guncelle({ literaturKaynaklari: [...tum, { tur, kaynak: '' }] })}
      >
        Kaynak ekle
      </button>
    </div>
  )
}

export function Adim5Icerik({ form, guncelle, uyari }: AdimProps) {
  return (
    <>
      <h2>Proje içeriği</h2>
      <AdimOzeti sayi={ADIM5_ALANLARI.flatMap(uyari).length} />

      <h3>1. Özet</h3>
      <UzunMetin
        etiket="Türkçe özet"
        ipucu="Amaç, yöntem, hedefler ve beklenen katkılar; ayrı paragraflar halinde kısa ve net."
        deger={form.turkceOzet}
        degisti={(v) => guncelle({ turkceOzet: v })}
        uyarilar={uyari('TurkceOzet')}
      />
      <Alan etiket="Anahtar kelimeler" uyarilar={uyari('AnahtarKelimeler')}>
        <input
          type="text"
          value={form.anahtarKelimeler ?? ''}
          onChange={(e) => guncelle({ anahtarKelimeler: bosIseNull(e.target.value) })}
        />
      </Alan>
      <UzunMetin
        etiket="Yabancı dilde özet"
        deger={form.yabanciDilOzet}
        degisti={(v) => guncelle({ yabanciDilOzet: v })}
        uyarilar={uyari('YabanciDilOzet')}
      />
      <Alan etiket="Keywords" uyarilar={uyari('Keywords')}>
        <input type="text" value={form.keywords ?? ''} onChange={(e) => guncelle({ keywords: bosIseNull(e.target.value) })} />
      </Alan>

      <h3>2. Amaç</h3>
      <UzunMetin
        etiket="Araştırmanın amacı"
        ipucu='Gönüllü katılım formunda "… amacını taşımaktadır" cümlesinde kullanılır.'
        deger={form.amac}
        degisti={(v) => guncelle({ amac: v })}
        uyarilar={uyari('Amac')}
      />

      <h3>3. Konu, kapsam ve literatür özeti</h3>
      <UzunMetin
        etiket="Konu, kapsam ve literatür özeti"
        ipucu="Ham liste değil, kısa bir literatür analizi; çalışmanın hangi eksikliği dolduracağı."
        deger={form.konuKapsamLiteratur}
        degisti={(v) => guncelle({ konuKapsamLiteratur: v })}
        uyarilar={uyari('KonuKapsamLiteratur')}
      />
      <LiteraturListesi tur="SecilmisLiteratur" baslik="Seçilmiş literatür listesi" form={form} guncelle={guncelle} />

      <h3>4. Materyal ve yöntem</h3>
      <UzunMetin
        etiket="Materyal ve yöntem"
        deger={form.materyalYontem}
        degisti={(v) => guncelle({ materyalYontem: v })}
        uyarilar={uyari('MateryalYontem')}
      />
      <LiteraturListesi tur="YontemLiteraturu" baslik="Yöntemlerde yararlanılan literatür" form={form} guncelle={guncelle} />

      <h3>5. Özgün değer</h3>
      <UzunMetin
        etiket="Özgün değer"
        deger={form.ozgunDeger}
        degisti={(v) => guncelle({ ozgunDeger: v })}
        uyarilar={uyari('OzgunDeger')}
      />

      <h3>6. Yaygın etki</h3>
      <UzunMetin
        etiket="Yaygın etki"
        deger={form.yayginEtki}
        degisti={(v) => guncelle({ yayginEtki: v })}
        uyarilar={uyari('YayginEtki')}
      />
    </>
  )
}

// ═══════════════════════ 6. İş paketleri ve riskler ═══════════════════════

export function Adim6IsPaketleri({ form, guncelle, uyari }: AdimProps) {
  const paketler = form.isPaketleri
  const riskler = form.riskler

  return (
    <>
      <h2>İş paketleri ve riskler</h2>

      <h3>7. Yönetim düzeni</h3>
      {paketler.map((p, i) => (
        <div className="satir" key={i}>
          <SatirBasligi baslik={`İP ${p.no}`} sil={() => guncelle({ isPaketleri: paketler.filter((_, j) => j !== i) })} />
          <Alan etiket="İş paketinin adı ve hedefleri">
            <input
              type="text"
              value={p.ad}
              onChange={(e) => guncelle({ isPaketleri: satirDegistir(paketler, i, { ad: e.target.value }) })}
            />
          </Alan>
          <div className="izgara">
            <Alan etiket="Kim(ler) tarafından">
              <input
                type="text"
                value={p.kimler ?? ''}
                onChange={(e) => guncelle({ isPaketleri: satirDegistir(paketler, i, { kimler: bosIseNull(e.target.value) }) })}
              />
            </Alan>
            <Alan etiket="Süre" ipucu="Örneğin: 1–3. aylar">
              <input
                type="text"
                value={p.sure ?? ''}
                onChange={(e) => guncelle({ isPaketleri: satirDegistir(paketler, i, { sure: bosIseNull(e.target.value) }) })}
              />
            </Alan>
          </div>
        </div>
      ))}
      <AlanUyarilari uyarilar={uyari('IsPaketleri')} />
      <button
        type="button"
        className="dugme ikincil kucuk"
        onClick={() =>
          guncelle({ isPaketleri: [...paketler, { no: paketler.length + 1, ad: '', kimler: null, sure: null }] })
        }
      >
        İş paketi ekle
      </button>

      <h3 style={{ marginTop: 28 }}>8. Risk yönetimi</h3>
      {riskler.map((r, i) => (
        <div className="satir" key={i}>
          <SatirBasligi baslik={`Risk ${i + 1}`} sil={() => guncelle({ riskler: riskler.filter((_, j) => j !== i) })} />
          <div className="izgara" style={{ gridTemplateColumns: '120px 1fr' }}>
            <Alan etiket="İP no">
              <select
                value={r.isPaketiNo}
                onChange={(e) => guncelle({ riskler: satirDegistir(riskler, i, { isPaketiNo: Number(e.target.value) }) })}
              >
                {(paketler.length ? paketler : [{ no: 1 }]).map((p) => (
                  <option key={p.no} value={p.no}>
                    {p.no}
                  </option>
                ))}
              </select>
            </Alan>
            <Alan etiket="En önemli risk">
              <input
                type="text"
                value={r.riskAciklamasi}
                onChange={(e) => guncelle({ riskler: satirDegistir(riskler, i, { riskAciklamasi: e.target.value }) })}
              />
            </Alan>
          </div>
          <Alan etiket="B planı">
            <input
              type="text"
              value={r.bPlani ?? ''}
              onChange={(e) => guncelle({ riskler: satirDegistir(riskler, i, { bPlani: bosIseNull(e.target.value) }) })}
            />
          </Alan>
        </div>
      ))}
      <AlanUyarilari uyarilar={uyari('Riskler')} />
      <button
        type="button"
        className="dugme ikincil kucuk"
        onClick={() => guncelle({ riskler: [...riskler, { isPaketiNo: paketler[0]?.no ?? 1, riskAciklamasi: '', bPlani: null }] })}
      >
        Risk ekle
      </button>
    </>
  )
}

// ═══════════════════════ 7. Etik sorular ═══════════════════════

const ADIM7_ALANLARI = [
  'YanlisBilgiVeyaGizlilikGerekiyorMu',
  'SaglikTehdidiVarMi',
  'KisilikHaklariKorunuyorMu',
  'ResitOlmayanKatilimciVarMi',
  'YeterliBilgiVeriliyorMu',
  'GonulluKatilimOnlemleri',
  'KatilmaCikmaKosullari',
  'BeklenenKatilimciSayisi',
  'KatilimciGruplari',
  'KatilimciGrubuDiger',
  'Uygulamalar',
  'UygulamaDiger',
]

function EtikSoru({
  no,
  soru,
  cevap,
  aciklama,
  aciklamaHangiCevapta,
  degisti,
  uyarilar,
}: {
  no: number
  soru: string
  cevap: boolean | null
  aciklama: string | null
  aciklamaHangiCevapta: boolean
  degisti: (cevap: boolean | null, aciklama: string | null) => void
  uyarilar: string[]
}) {
  return (
    <div className="soru">
      <span className="etiket">
        {no}. {soru}
      </span>
      <EvetHayir etiket={`Soru ${no}`} deger={cevap} degisti={(d) => degisti(d, aciklama)} />
      {cevap === aciklamaHangiCevapta ? (
        <div style={{ marginTop: 12 }}>
          <Alan etiket={`${aciklamaHangiCevapta ? 'Evet' : 'Hayır'} ise açıklayın`} uyarilar={uyarilar}>
            <textarea
              value={aciklama ?? ''}
              style={{ minHeight: 70 }}
              onChange={(e) => degisti(cevap, bosIseNull(e.target.value))}
            />
          </Alan>
        </div>
      ) : (
        <AlanUyarilari uyarilar={uyarilar} />
      )}
    </div>
  )
}

export function Adim7Etik({ form, guncelle, uyari, secenekler }: AdimProps & { secenekler: Secenek[] }) {
  const cocukGrubuSecili = secenekler.some(
    (s) => s.cocukRizasiGerektirirMi && form.secenekIdleri.includes(s.id),
  )

  return (
    <>
      <h2>Etik değerlendirme soruları</h2>
      <AdimOzeti sayi={ADIM7_ALANLARI.flatMap(uyari).length} />

      <EtikSoru
        no={9}
        soru="Araştırma katılımcılara herhangi bir şekilde yanlı/yanlış bilgi vermeyi ya da çalışmanın amacını tamamen gizli tutmayı gerektiriyor mu?"
        cevap={form.yanlisBilgiVeyaGizlilikGerekiyorMu}
        aciklama={form.yanlisBilgiVeyaGizlilikAciklama}
        aciklamaHangiCevapta={true}
        degisti={(c, a) => guncelle({ yanlisBilgiVeyaGizlilikGerekiyorMu: c, yanlisBilgiVeyaGizlilikAciklama: a })}
        uyarilar={uyari('YanlisBilgiVeyaGizlilikGerekiyorMu')}
      />
      <EtikSoru
        no={10}
        soru="Araştırma katılımcıların fiziksel veya ruhsal sağlıklarını tehdit edici sorular içeriyor mu?"
        cevap={form.saglikTehdidiVarMi}
        aciklama={form.saglikTehdidiAciklama}
        aciklamaHangiCevapta={true}
        degisti={(c, a) => guncelle({ saglikTehdidiVarMi: c, saglikTehdidiAciklama: a })}
        uyarilar={uyari('SaglikTehdidiVarMi')}
      />
      <EtikSoru
        no={11}
        soru="Katılımcıların kişilik hakları ve özel bilgileri korunmakta mıdır?"
        cevap={form.kisilikHaklariKorunuyorMu}
        aciklama={form.kisilikHaklariAciklama}
        aciklamaHangiCevapta={true}
        degisti={(c, a) => guncelle({ kisilikHaklariKorunuyorMu: c, kisilikHaklariAciklama: a })}
        uyarilar={uyari('KisilikHaklariKorunuyorMu')}
      />
      <EtikSoru
        no={12}
        soru="Araştırmaya reşit olmayan çocuklar, kısıtlı ya da engelliler katılmakta mıdır?"
        cevap={form.resitOlmayanKatilimciVarMi}
        aciklama={form.resitOlmayanKatilimciAciklama}
        aciklamaHangiCevapta={true}
        degisti={(c, a) => guncelle({ resitOlmayanKatilimciVarMi: c, resitOlmayanKatilimciAciklama: a })}
        uyarilar={uyari('ResitOlmayanKatilimciVarMi')}
      />
      <EtikSoru
        no={13}
        soru="Katılımcılara araştırmanın niteliği hakkında yeterince açık ve anlaşılabilir bilgi verilmekte midir (gönüllü katılım formu)?"
        cevap={form.yeterliBilgiVeriliyorMu}
        aciklama={form.yeterliBilgiAciklama}
        aciklamaHangiCevapta={false}
        degisti={(c, a) => guncelle({ yeterliBilgiVeriliyorMu: c, yeterliBilgiAciklama: a })}
        uyarilar={uyari('YeterliBilgiVeriliyorMu')}
      />

      <div className="soru">
        <Alan
          etiket="14. Gönüllü katılımı bozacak ve katılımcıları suistimal edecek tehditlere karşı ne gibi önlemler alınmıştır?"
          uyarilar={uyari('GonulluKatilimOnlemleri')}
        >
          <textarea
            value={form.gonulluKatilimOnlemleri ?? ''}
            style={{ minHeight: 70 }}
            onChange={(e) => guncelle({ gonulluKatilimOnlemleri: bosIseNull(e.target.value) })}
          />
        </Alan>
        <Alan
          etiket="15. Gönüllülerin katılma ve çıkma/çıkarılma koşulları açık ve net olarak belli midir?"
          uyarilar={uyari('KatilmaCikmaKosullari')}
        >
          <textarea
            value={form.katilmaCikmaKosullari ?? ''}
            style={{ minHeight: 70 }}
            onChange={(e) => guncelle({ katilmaCikmaKosullari: bosIseNull(e.target.value) })}
          />
        </Alan>
        <div style={{ maxWidth: 320 }}>
          <Alan etiket="16. Beklenen katılımcı sayısı (tahmini)" uyarilar={uyari('BeklenenKatilimciSayisi')}>
            <input
              type="number"
              min={1}
              value={form.beklenenKatilimciSayisi ?? ''}
              onChange={(e) => guncelle({ beklenenKatilimciSayisi: sayiVeyaNull(e.target.value) })}
            />
          </Alan>
        </div>
      </div>

      <div className="soru">
        <span className="etiket">17. Katılımcıları en iyi tanımlayan seçenekler</span>
        <CokluSecim grup="KatilimciGrubu" secenekler={secenekler} form={form} guncelle={guncelle} />
        <AlanUyarilari uyarilar={uyari('KatilimciGruplari')} />
        {cocukGrubuSecili && (
          <div style={{ marginTop: 12 }}>
            <Bildirim
              tur="bilgi"
              mesaj="18 yaş altı bir grup seçtiniz: 12. soruyu 'Evet' olarak cevaplayın. Ek-3 Çocuk Rıza Formu istenecek."
            />
          </div>
        )}
        {digerSecildi('KatilimciGrubu', secenekler, form.secenekIdleri) && (
          <div style={{ marginTop: 12 }}>
            <Alan etiket="Diğer katılımcı grubunu belirtin" uyarilar={uyari('KatilimciGrubuDiger')}>
              <input
                type="text"
                value={form.katilimciGrubuDiger ?? ''}
                onChange={(e) => guncelle({ katilimciGrubuDiger: bosIseNull(e.target.value) })}
              />
            </Alan>
          </div>
        )}
      </div>

      <div className="soru">
        <span className="etiket">18. Çalışma kapsamında yer alacak uygulamalar</span>
        <CokluSecim grup="Uygulama" secenekler={secenekler} form={form} guncelle={guncelle} />
        <AlanUyarilari uyarilar={uyari('Uygulamalar')} />
        {digerSecildi('Uygulama', secenekler, form.secenekIdleri) && (
          <div style={{ marginTop: 12 }}>
            <Alan etiket="Diğer uygulamayı belirtin" uyarilar={uyari('UygulamaDiger')}>
              <input
                type="text"
                value={form.uygulamaDiger ?? ''}
                onChange={(e) => guncelle({ uygulamaDiger: bosIseNull(e.target.value) })}
              />
            </Alan>
          </div>
        )}
      </div>
    </>
  )
}
