import { useState } from 'react';
import { listarCafesVerdes } from '../api/cafe';
import { crearVenta, listarClientes, listarPresentaciones, Venta } from '../api/ventas';
import { aNumero, CampoKg } from '../components/CampoKg';
import { CampoFecha } from '../components/CampoFecha';
import { Campo, Formulario } from '../components/Formulario';
import { SelectorId } from '../components/SelectorId';
import { useListado } from '../components/useListado';
import { formatearKg, formatearMoneda, formatearMonedaKg, formatearPct, hoyIso } from '../format/formato';

export function Ventas() {
  const clientes = useListado(listarClientes);
  const cafes = useListado(listarCafesVerdes);
  const presentaciones = useListado(listarPresentaciones);
  const [clienteId, setClienteId] = useState<number | null>(null);
  const [cafeId, setCafeId] = useState<number | null>(null);
  const [presentacionId, setPresentacionId] = useState<number | null>(null);
  const [unidades, setUnidades] = useState('');
  const [precio, setPrecio] = useState('');
  const [fecha, setFecha] = useState(hoyIso());
  const [resultado, setResultado] = useState<Venta | null>(null);

  const enviar = async () => {
    setResultado(null);
    const venta = await crearVenta({
      clienteId,
      cafeVerdeId: cafeId,
      presentacionId,
      unidades: unidades === '' ? null : Number(unidades),
      precioUnitario: aNumero(precio),
      fecha,
    });
    setResultado(venta);
    setUnidades('');
    setPrecio('');
  };

  return (
    <section>
      <h1>Ventas</h1>
      <Formulario titulo="Registrar venta" enviar={enviar}>
        <Campo etiqueta="Cliente" nombre="clienteId">
          <SelectorId opciones={clientes.filas} valor={clienteId} onChange={setClienteId} />
        </Campo>
        <Campo etiqueta="Café vendido" nombre="cafeVerdeId">
          <SelectorId opciones={cafes.filas} valor={cafeId} onChange={setCafeId} />
        </Campo>
        <Campo etiqueta="Presentación" nombre="presentacionId">
          <SelectorId opciones={presentaciones.filas} valor={presentacionId} onChange={setPresentacionId} />
        </Campo>
        <Campo etiqueta="Unidades" nombre="unidades">
          <input
            aria-label="Unidades"
            inputMode="numeric"
            value={unidades}
            onChange={(e) => /^\d*$/.test(e.target.value) && setUnidades(e.target.value)}
          />
        </Campo>
        <Campo etiqueta="Precio por unidad ($)" nombre="precioUnitario">
          <CampoKg aria-label="Precio por unidad ($)" valor={precio} onChange={setPrecio} decimales={2} />
        </Campo>
        <Campo etiqueta="Fecha" nombre="fecha">
          <CampoFecha valor={fecha} onChange={setFecha} />
        </Campo>
      </Formulario>
      {resultado && <ResultadoVenta venta={resultado} />}
    </section>
  );
}

/** Muestra los valores tal como los calculó y guardó la API, sin recalcular. */
export function ResultadoVenta({ venta }: { venta: Venta }) {
  const negativo = venta.ganancia < 0 ? 'negativo' : undefined;
  return (
    <div className="resultado" aria-label="Resultado de la venta">
      <h2>Venta registrada</h2>
      <dl>
        <dt>Café vendido</dt>
        <dd>
          {venta.cafeVerde.nombre} — {formatearKg(venta.cantidadVendidaKg)}
        </dd>
        <dt>Ingreso</dt>
        <dd>{formatearMoneda(venta.ingreso)}</dd>
        <dt>Costo del café vendido</dt>
        <dd>
          {formatearMoneda(venta.costoCafeVendido)} (a {formatearMonedaKg(venta.costoPromedioKgAplicado)})
        </dd>
        <dt>Costos asociados</dt>
        <dd>{formatearMoneda(venta.costosAsociados)}</dd>
        <dt>Ganancia</dt>
        <dd className={negativo}>{formatearMoneda(venta.ganancia)}</dd>
        <dt>Margen</dt>
        <dd className={negativo}>{formatearPct(venta.margenPct)}</dd>
      </dl>
    </div>
  );
}
