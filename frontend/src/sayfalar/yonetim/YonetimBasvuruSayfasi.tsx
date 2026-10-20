// Yetkilinin başvuruyu incelediği ve işlem yaptığı sayfa.
// Sağdaki "İşlem" kutusu, kullanıcının rolüne ve başvurunun durumuna göre değişir:
//   Arşiv Kayıt  → tarih + sayı ver, arşive ekle, yetkili birime gönder
//   Yetkili Birim → onayla ve etik kurula gönder / kararı başvurana ilet
//   Etik Kurul   → Onaylıyorum / Onaylamıyorum

import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useParams } from 'react-router-dom'
import { BASVURU_TURU_ADLARI, NITELIK_ADLARI, tarihSaat } from '../../api/etiketler'
import { hataMesaji } from '../../api/istemci'
import { yonetimApi } from '../../api/servisler'
import type { YonetimDetay } from '../../api/tipler'
import { BasvuruTakibi, KararKutusu } from '../../bilesenler/BasvuruTakibi'
import { Alan, Bildirim, DurumKasesi, GeriBaglantisi } from '../../bilesenler/Ortak'

/** Tarih kutusu (datetime-local) için şimdiki zaman: "2026-10-20T14:30" */
function simdiYerel(): string {
  const d = new Date()
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset())
  return d.toISOString().slice(0, 16)
}

export function YonetimBasvuruSayfasi() {
  const id = Number(useParams().id)
  const [detay, setDetay] = useState<YonetimDetay | null>(null)
  const [hata, setHata] = useState<{ mesaj: string; ayrintilar: string[] } | null>(null)
  const [basari, setBasari] = useState<string | null>(null)
  const [isleniyor, setIsleniyor] = useState(false)

  const yukle = useCallback(async () => {
    try {
      setDetay(await yonetimApi.detay(id))
    } catch (e) {
      setHata(hataMesaji(e))
    }
  }, [id])

  useEffect(() => {
    void yukle()
  }, [yukle])

  async function islem(fn: () => Promise<{ mesaj: string }>, onay?: string) {
    if (onay && !confirm(onay)) return
    setIsleniyor(true)
    setHata(null)
    setBasari(null)
    try {
      const c = await fn()
      setBasari(c.mesaj)
      await yukle()
      window.scrollTo({ top: 0 })
    } catch (e) {
      setHata(hataMesaji(e))
    } finally {
      setIsleniyor(false)
    }
  }

  if (!detay) {
    return (
      <>
        <GeriBaglantisi to="/gorevler" metin="Görevlerim" />
        {hata ? <Bildirim tur="hata" mesaj={hata.mesaj} /> : <p className="bos">Başvuru yükleniyor…</p>}
      </>
    )
  }

  const b = detay.basvuru
  const sorumlu = b.arastirmacilar.find((a) => a.rol === 'SorumluArastirmaci')

  return (
    <>
      <GeriBaglantisi to="/gorevler" metin="Görevlerim" />
      <div className="sayfa-basligi">
        <div>
          <p>
            Başvuru no {b.id} · {BASVURU_TURU_ADLARI[b.basvuruTuru]}
            {b.evrakKayitNo && <> · Arşiv kayıt sayısı {b.evrakKayitNo}</>}
          </p>
          <h1 style={{ marginTop: 6 }}>{b.arastirmaAdi}</h1>
        </div>
        <DurumKasesi durum={b.durum} buyuk kabul={detay.karar ? detay.karar.kararTuru === 'Uygun' : null} />
      </div>

      {basari && <Bildirim tur="basari" mesaj={basari} />}
      {hata && <Bildirim tur="hata" mesaj={hata.mesaj} ayrintilar={hata.ayrintilar} />}

      <div className="inceleme">
        <div>
          <section className="kart">
            <h2>Başvuru</h2>
            <dl className="bilgi-listesi">
              <dt>Başvuran</dt>
              <dd>
                {detay.basvuran.adSoyad} · {detay.basvuran.calistigiBirim}
              </dd>
              <dt>İletişim</dt>
              <dd>
                {detay.basvuran.kurumsalEmail}
                {detay.basvuran.telefon && ` · ${detay.basvuran.telefon}`}
              </dd>
              <dt>Araştırmanın niteliği</dt>
              <dd>{b.arastirmaNiteligi ? NITELIK_ADLARI[b.arastirmaNiteligi] : '-'}</dd>
              <dt>Sorumlu araştırmacı</dt>
              <dd>{sorumlu ? `${sorumlu.unvan ?? ''} ${sorumlu.adSoyad}`.trim() : '-'}</dd>
              <dt>Veri toplama dönemi</dt>
              <dd>
                {b.veriToplamaBaslangic ?? '-'} / {b.veriToplamaBitis ?? '-'}
              </dd>
              <dt>Gönderim tarihi</dt>
              <dd>{tarihSaat(b.gonderimTarihi)}</dd>
            </dl>
            {b.amac && (
              <>
                <h3>Amaç</h3>
                <p>{b.amac}</p>
              </>
            )}
            <button className="dugme ikincil" onClick={() => yonetimApi.pdfAc(b.id).catch((e) => setHata(hataMesaji(e)))}>
              Başvuru formunu (PDF) aç
            </button>
          </section>

          <section className="kart">
            <h2>Yüklenen imzalı belgeler ({detay.ekler.length})</h2>
            {detay.ekler.length === 0 ? (
              <p className="meta">Belge yok.</p>
            ) : (
              <ul className="yuklenenler">
                {detay.ekler.map((e) => {
                  const kisi = e.basvuruArastirmacisiId
                    ? b.arastirmacilar.find((a) => a.id === e.basvuruArastirmacisiId)?.adSoyad
                    : null
                  return (
                    <li key={e.id}>
                      <strong style={{ minWidth: 260 }}>
                        {e.ekTuruAd}
                        {kisi && ` (${kisi})`}
                      </strong>
                      <button
                        className="dugme metin kucuk"
                        onClick={() => yonetimApi.ekAc(b.id, e.id).catch((err) => setHata(hataMesaji(err)))}
                      >
                        {e.orijinalDosyaAdi}
                      </button>
                    </li>
                  )
                })}
              </ul>
            )}
          </section>

          <section className="kart">
            <h2>Başvurunun geçmişi</h2>
            <BasvuruTakibi
              hareketler={detay.hareketler}
              evrakKayitNo={b.evrakKayitNo}
              evrakKayitTarihi={b.evrakKayitTarihi}
            />
          </section>
        </div>

        <aside>
          <IslemKutusu detay={detay} isleniyor={isleniyor} islem={islem} />
        </aside>
      </div>
    </>
  )
}

function IslemKutusu({
  detay,
  isleniyor,
  islem,
}: {
  detay: YonetimDetay
  isleniyor: boolean
  islem: (fn: () => Promise<{ mesaj: string }>, onay?: string) => Promise<void>
}) {
  const b = detay.basvuru
  const yapabilir = (i: YonetimDetay['islemler'][number]) => detay.islemler.includes(i)

  return (
    <section className="kart islem-kutusu">
      <h2>İşlem</h2>

      {detay.karar && <KararKutusu karar={detay.karar} />}

      {detay.oylama && !detay.karar && (
        <p>
          <strong>Etik kurul oylaması:</strong> {detay.oylama.oyVerenSayisi} / {detay.oylama.uyeSayisi} üye oy verdi.
        </p>
      )}
      {detay.oylama?.benimOyum !== null && detay.oylama?.benimOyum !== undefined && (
        <Bildirim tur="bilgi" mesaj={`Oyunuz: ${detay.oylama.benimOyum ? 'Onaylıyorum' : 'Onaylamıyorum'}`} />
      )}

      {yapabilir('ArsiveKaydet') && <ArsivKayitFormu basvuruId={b.id} isleniyor={isleniyor} islem={islem} />}

      {yapabilir('EtikKuruluGonder') && (
        <>
          <p>Başvuruyu onaylayıp etik kurulun 3 üyesine aynı anda gönderin.</p>
          <button
            className="dugme"
            disabled={isleniyor}
            onClick={() =>
              islem(
                () => yonetimApi.etikKurulaGonder(b.id),
                'Başvuru onaylanıp etik kurulun 3 üyesine gönderilecek. Devam edilsin mi?',
              )
            }
          >
            Onayla ve etik kurula gönder
          </button>
        </>
      )}

      {yapabilir('OyVer') && (
        <>
          <p>Başvuruyu ve belgeleri inceledikten sonra oyunuzu verin. Oyunuz sonradan değiştirilemez.</p>
          <div className="dugmeler">
            <button
              className="dugme"
              disabled={isleniyor}
              onClick={() => islem(() => yonetimApi.oyVer(b.id, true), 'Oyunuz: ONAYLIYORUM. Kaydedilsin mi?')}
            >
              Onaylıyorum
            </button>
            <button
              className="dugme tehlike"
              disabled={isleniyor}
              onClick={() => islem(() => yonetimApi.oyVer(b.id, false), 'Oyunuz: ONAYLAMIYORUM. Kaydedilsin mi?')}
            >
              Onaylamıyorum
            </button>
          </div>
        </>
      )}

      {yapabilir('BasvuranaIlet') && (
        <>
          <p>Etik kurul kararını başvurana iletin. Başvuran sonucu kendi ekranında görecek.</p>
          <button
            className="dugme"
            disabled={isleniyor}
            onClick={() =>
              islem(() => yonetimApi.basvuranaIlet(b.id), 'Etik kurul kararı başvurana iletilecek. Devam edilsin mi?')
            }
          >
            Kararı başvurana ilet
          </button>
        </>
      )}

      {detay.islemler.length === 0 && !detay.karar && (
        <p className="meta">Bu aşamada sizin yapmanız gereken bir işlem yok.</p>
      )}
    </section>
  )
}

function ArsivKayitFormu({
  basvuruId,
  isleniyor,
  islem,
}: {
  basvuruId: number
  isleniyor: boolean
  islem: (fn: () => Promise<{ mesaj: string }>, onay?: string) => Promise<void>
}) {
  const [kayitNo, setKayitNo] = useState('')
  const [tarih, setTarih] = useState(simdiYerel())

  useEffect(() => {
    yonetimApi
      .onerilenKayitNo()
      .then((c) => setKayitNo((k) => k || c.kayitNo))
      .catch(() => {})
  }, [])

  function gonder(e: FormEvent) {
    e.preventDefault()
    void islem(
      () => yonetimApi.arsiveKaydet(basvuruId, kayitNo.trim(), new Date(tarih).toISOString()),
      `Başvuru ${kayitNo} sayısıyla arşive kaydedilip yetkili birime gönderilecek. Devam edilsin mi?`,
    )
  }

  return (
    <form onSubmit={gonder}>
      <p>Başvuruya kayıt tarihi ve sayısı verin. Başvuru arşive eklenir ve yetkili birime gönderilir.</p>
      <Alan etiket="Kayıt sayısı" ipucu="Sistem sıradaki sayıyı önerir; isterseniz değiştirebilirsiniz.">
        <input type="text" value={kayitNo} onChange={(e) => setKayitNo(e.target.value)} required maxLength={50} />
      </Alan>
      <Alan etiket="Kayıt tarihi ve saati">
        <input type="datetime-local" value={tarih} onChange={(e) => setTarih(e.target.value)} required />
      </Alan>
      <button className="dugme" disabled={isleniyor || !kayitNo.trim()}>
        Arşive kaydet ve yetkili birime gönder
      </button>
    </form>
  )
}
