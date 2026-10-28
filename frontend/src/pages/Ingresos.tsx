import { useState } from 'react';
import { crearIngreso, Ingreso, listarCafesVerdes, listarIngresos } from '../api/cafe';
import { aNumero, CampoKg } from '../components/CampoKg';
import { CampoFecha } from '../components/CampoFecha';
import { Campo, Formulario } from '../components/Formulario';
import { SelectorId } from '../components/SelectorId';
import { Tabla } from '../components/Tabla';
import { useListado } from '../components/useListado';
import { formatearFecha, formatearKg, hoyIso } from '../format/formato';

export function Ingresos() {
  const ingresos = useListado(() => listarIngresos());
  const cafes = useListado(listarCafesVerdes);
  const [cafeId, setCafeId] = useState<number | null>(null);
  const [cantidad, setCantidad] = useState('');
  const [fecha, setFecha] = useState(hoyIso());
  const [ultimo, setUltimo] = useState<Ingreso | null>(null);

  const enviar = async () => {
    setUltimo(null);
    const ingreso = await crearIngreso({ cafeVerdeId: cafeId, cantidadKg: aNumero(cantidad), fecha });
    setUltimo(ingreso);
    setCantidad('');
    await ingresos.recargar();
  };

  return (
    <section>
      <h1>Ingresos de café verde</h1>
      <Formulario titulo="Registrar ingreso" enviar={enviar}>
        <Campo etiqueta="Café verde" nombre="cafeVerdeId">
          <SelectorId opciones={cafes.filas} valor={cafeId} onChange={setCafeId} />
        </Campo>
        <Campo etiqueta="Cantidad (kg)" nombre="cantidadKg">
          <CampoKg valor={cantidad} onChange={setCantidad} />
        </Campo>
        <Campo etiqueta="Fecha" nombre="fecha">
          <CampoFecha valor={fecha} onChange={setFecha} />
        </Campo>
      </Formulario>
      {ultimo && ultimo.stockVerdeResultanteKg != null && (
        <p className="resultado">
          Ingreso registrado. Stock verde de {ultimo.cafeVerde}: {formatearKg(ultimo.stockVerdeResultanteKg)}.
        </p>
      )}
      <h2>Historial</h2>
      <Tabla
        filas={ingresos.filas}
        clave={(i) => i.id}
        columnas={[
          { titulo: 'Fecha', valor: (i) => formatearFecha(i.fecha) },
          { titulo: 'Café', valor: (i) => i.cafeVerde },
          { titulo: 'Cantidad', valor: (i) => formatearKg(i.cantidadKg), numerica: true },
        ]}
      />
    </section>
  );
}
