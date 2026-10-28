import { useState } from 'react';
import { crearPresentacion, listarInsumos, listarPresentaciones } from '../../api/ventas';
import { Campo, Formulario } from '../../components/Formulario';
import { Tabla } from '../../components/Tabla';
import { useListado } from '../../components/useListado';

export function Presentaciones() {
  const presentaciones = useListado(listarPresentaciones);
  const insumos = useListado(listarInsumos);
  const [nombre, setNombre] = useState('');
  const [gramos, setGramos] = useState('');
  const [insumoIds, setInsumoIds] = useState<number[]>([]);

  const alternar = (id: number) =>
    setInsumoIds((ids) => (ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id]));

  const enviar = async () => {
    await crearPresentacion({ nombre, gramos: gramos === '' ? null : Number(gramos), insumoIds });
    setNombre('');
    setGramos('');
    setInsumoIds([]);
    await presentaciones.recargar();
  };

  return (
    <section>
      <h1>Presentaciones de venta</h1>
      <Formulario titulo="Nueva presentación" enviar={enviar}>
        <Campo etiqueta="Nombre" nombre="nombre">
          <input value={nombre} onChange={(e) => setNombre(e.target.value)} />
        </Campo>
        <Campo etiqueta="Gramos por unidad" nombre="gramos">
          <input
            inputMode="numeric"
            value={gramos}
            onChange={(e) => /^\d*$/.test(e.target.value) && setGramos(e.target.value)}
          />
        </Campo>
        <Campo etiqueta="Insumos por unidad" nombre="insumoIds">
          <div>
            {insumos.filas.length === 0 && <small>No hay insumos cargados.</small>}
            {insumos.filas.map((i) => (
              <label key={i.id} style={{ display: 'block' }}>
                <input type="checkbox" checked={insumoIds.includes(i.id)} onChange={() => alternar(i.id)} /> {i.nombre}
              </label>
            ))}
          </div>
        </Campo>
      </Formulario>
      <Tabla
        filas={presentaciones.filas}
        clave={(p) => p.id}
        columnas={[
          { titulo: 'Nombre', valor: (p) => p.nombre },
          { titulo: 'Gramos', valor: (p) => `${p.gramos} g`, numerica: true },
          { titulo: 'Insumos', valor: (p) => p.insumos.map((i) => i.nombre).join(', ') || '—' },
        ]}
      />
    </section>
  );
}
