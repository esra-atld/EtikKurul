// 8. adım: eksik kontrolü, taslak PDF ve formu kilitleme.

import { useEffect, useState } from 'react'
import { hataMesaji } from '../../api/istemci'
import { basvuruApi } from '../../api/servisler'
import type { BasvuruDetay, EksikAlan } from '../../api/tipler'
import { Bildirim } from '../../bilesenler/Ortak'
import { ADIM_ADLARI } from './adimlar-listesi'

export function Adim8Onizleme({
  basvuru,
  adimaGit,
  eksiklerDegisti,
  yenile,
}: {
  basvuru: BasvuruDetay
  adimaGit: (n: number) => void
  eksiklerDegisti: (e: EksikAlan[]) => void
  yenile: () => Promise<void>
}) {
  const [eksikler, setEksikler] = useState<EksikAlan[] | null>(null)
  const [hata, setHata] = useState<string | null>(null)
  const [isleniyor, setIsleniyor] = useState(false)

  useEffect(() => {
    if (!basvuru.duzenlenebilirMi) return
    basvuruApi
      .eksikler(basvuru.id)
      .then((c) => {
        setEksikler(c.eksikler)
        eksiklerDegisti(c.eksikler)
      })
      .catch((e) => setHata(hataMesaji(e).mesaj))
  }, [basvuru.id, basvuru.duzenlenebilirMi, eksiklerDegisti])

  async function pdfGoster() {
    setHata(null)
    try {
      await basvuruApi.pdfAc(basvuru.id)
    } catch (e) {
      setHata(hataMesaji(e).mesaj)
    }
  }

  async function kilitle() {
    if (
      !confirm(
        'Form kilitlenecek ve artık değiştirilemeyecek. PDF çıktısını alıp imzalayacaksınız.\n\nDevam edilsin mi?',
      )
    )
      return
    setIsleniyor(true)
    setHata(null)
    try {
      await basvuruApi.kilitle(basvuru.id)
      await yenile()
      adimaGit(9)
    } catch (e) {
      setHata(hataMesaji(e).mesaj)
    } finally {
      setIsleniyor(false)
    }
  }

  // Kilitli veya gönderilmiş başvuru
  if (!basvuru.duzenlenebilirMi) {
    return (
      <>
        <h2>Önizleme ve çıktı</h2>
        <Bildirim tur="bilgi" mesaj="Form kilitli. Aşağıdaki PDF, imzalanacak resmi çıktıdır." />
        {hata && <Bildirim tur="hata" mesaj={hata} />}
        <button className="dugme" onClick={pdfGoster}>
          Başvuru paketini (PDF) aç
        </button>
      </>
    )
  }

  // Eksikleri adım adım grupla
  const adimlara = new Map<number, EksikAlan[]>()
  for (const e of eksikler ?? []) adimlara.set(e.adim, [...(adimlara.get(e.adim) ?? []), e])

  return (
    <>
      <h2>Önizleme ve çıktı</h2>
      <p>
        Sistem formunuzdaki bilgilerle dilekçeyi, başvuru formunu, Ek-1 taahhütnameyi ve Ek-2 gönüllü katılım formunu
        tek bir PDF olarak hazırlar. Kilitlemeden önce taslağı inceleyebilirsiniz.
      </p>

      {hata && <Bildirim tur="hata" mesaj={hata} />}

      {eksikler === null && !hata && <p className="bos">Form kontrol ediliyor…</p>}

      {eksikler && eksikler.length > 0 && (
        <div className="kart" style={{ borderColor: 'var(--kase-kirmizi)', marginBottom: 16 }}>
          <h3>Kilitlemeden önce tamamlanması gerekenler ({eksikler.length})</h3>
          <p style={{ color: 'var(--soluk)', fontSize: '0.92rem' }}>
            "Düzelt" ile ilgili adıma gidin; eksik alanlar orada kırmızı uyarıyla işaretlidir.
          </p>
          <ul className="eksik-listesi">
            {[...adimlara.entries()]
              .sort(([a], [b]) => a - b)
              .flatMap(([adim, liste]) =>
                liste.map((e, i) => (
                  <li key={`${adim}-${i}`}>
                    <span className="adim-etiketi">{ADIM_ADLARI[adim - 1]}</span>
                    <span style={{ flex: 1 }}>{e.mesaj}</span>
                    <button className="dugme metin kucuk" onClick={() => adimaGit(adim)}>
                      Düzelt
                    </button>
                  </li>
                )),
              )}
          </ul>
        </div>
      )}

      {eksikler && eksikler.length === 0 && (
        <Bildirim tur="basari" mesaj="Form eksiksiz. Taslağı kontrol edip formu kilitleyebilirsiniz." />
      )}

      <div className="dugmeler">
        <button className="dugme ikincil" onClick={pdfGoster}>
          Taslak PDF'i görüntüle
        </button>
        <button className="dugme" onClick={kilitle} disabled={isleniyor || !eksikler || eksikler.length > 0}>
          {isleniyor ? 'Kilitleniyor…' : 'Formu kilitle ve imzaya hazırla'}
        </button>
      </div>
      <p style={{ color: 'var(--soluk)', fontSize: '0.9rem', marginTop: 12 }}>
        Kilitledikten sonra form değiştirilemez; çünkü imzaladığınız kağıt ile sistemdeki bilgiler aynı kalmalı.
        Değişiklik gerekirse kilidi açabilirsiniz, ama o zaman yüklediğiniz belgeler silinir.
      </p>
    </>
  )
}
