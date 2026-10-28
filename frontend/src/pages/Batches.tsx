import { useState } from 'react';
import { Batch, crearBatch, listarCafesVerdes } from '../api/cafe';
import { aNumero, CampoKg } from '../components/CampoKg';
import { CampoFecha } from '../components/CampoFecha';
import { Campo, Formulario } from '../components/Formulario';
import { SelectorId } from '../components/SelectorId';
import { useListado } from '../components/useListado';
import { formatearDecimal, formatearKg, formatearMoneda, formatearMonedaKg, formatearPct, hoyIso } from '../format/formato';

export function Batches() {
  const cafes = useListado(listarCafesVerdes);
  const [cafeId, setCafeId] = useState<number | null>(null);
  const [verde, setVerde] = useState('');
  const [tostado, setTostado] = useState('');
  const [fecha, setFecha] = useState(hoyIso());
  const [humedad, setHumedad] = useState('');
  const [densidad, setDensidad] = useState('');
  const [desarrollo, setDesarrollo] = useState('');
  const [resultado, setResultado] = useState<Batch | null>(null);

  const enviar = async () => {
    setResultado(null);
    const batch = await crearBatch({
      cafeVerdeId: cafeId,
      verdeKg: aNumero(verde),
      tostadoKg: aNumero(tostado),
      fecha,
      humedadPct: aNumero(humedad),
      densidadGl: aNumero(densidad),
      desarrolloPct: aNumero(desarrollo),
    });
    setResultado(batch);
    setVerde('');
    setTostado('');
    setHumedad('');
    setDensidad('');
    setDesarrollo('');
  };

  return (
    <section>
      <h1>Batches de tueste</h1>
      <Formulario titulo="Registrar batch" enviar={enviar}>
        <Campo etiqueta="Café verde" nombre="cafeVerdeId">
          <SelectorId opciones={cafes.filas} valor={cafeId} onChange={setCafeId} />
        </Campo>
        <Campo etiqueta="Café verde utilizado (kg)" nombre="verdeKg">
          <CampoKg aria-label="Café verde utilizado (kg)" valor={verde} onChange={setVerde} />
        </Campo>
        <Campo etiqueta="Café tostado obtenido (kg)" nombre="tostadoKg">
          <CampoKg aria-label="Café tostado obtenido (kg)" valor={tostado} onChange={setTostado} />
        </Campo>
        <Campo etiqueta="Fecha" nombre="fecha">
          <CampoFecha valor={fecha} onChange={setFecha} />
        </Campo>
        <Campo etiqueta="Humedad (%) — opcional" nombre="humedadPct">
          <CampoKg valor={humedad} onChange={setHumedad} decimales={2} />
        </Campo>
        <Campo etiqueta="Densidad (g/L) — opcional" nombre="densidadGl">
          <CampoKg valor={densidad} onChange={setDensidad} decimales={2} />
        </Campo>
        <Campo etiqueta="Desarrollo (%) — opcional" nombre="desarrolloPct">
          <CampoKg valor={desarrollo} onChange={setDesarrollo} decimales={2} />
        </Campo>
      </Formulario>
      {resultado && (
        <div className="resultado" aria-label="Resultado del batch">
          <h2>Batch registrado</h2>
          <dl>
            <dt>Café</dt>
            <dd>{resultado.cafeVerde}</dd>
            <dt>Verde / tostado</dt>
            <dd>
              {formatearKg(resultado.verdeKg)} → {formatearKg(resultado.tostadoKg)}
            </dd>
            <dt>Merma</dt>
            <dd>{formatearPct(resultado.mermaPct)}</dd>
            <dt>Precio del verde aplicado</dt>
            <dd>{formatearMonedaKg(resultado.precioKgAplicado)}</dd>
            <dt>Costo del tostado</dt>
            <dd>{formatearMonedaKg(resultado.costoTostadoKg)}</dd>
            <dt>Costo total del batch</dt>
            <dd>{formatearMoneda(resultado.costoTotal)}</dd>
            {resultado.humedadPct != null && (
              <>
                <dt>Humedad</dt>
                <dd>{formatearPct(resultado.humedadPct)}</dd>
              </>
            )}
            {resultado.densidadGl != null && (
              <>
                <dt>Densidad</dt>
                <dd>{formatearDecimal(resultado.densidadGl)} g/L</dd>
              </>
            )}
            {resultado.desarrolloPct != null && (
              <>
                <dt>Desarrollo</dt>
                <dd>{formatearPct(resultado.desarrolloPct)}</dd>
              </>
            )}
          </dl>
        </div>
      )}
    </section>
  );
}
