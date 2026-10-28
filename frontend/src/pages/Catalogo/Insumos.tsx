import { useState } from 'react';
import { crearInsumo, listarInsumos, listarProveedoresInsumo } from '../../api/ventas';
import { aNumero, CampoKg } from '../../components/CampoKg';
import { Campo, Formulario } from '../../components/Formulario';
import { SelectorId } from '../../components/SelectorId';
import { Tabla } from '../../components/Tabla';
import { useListado } from '../../components/useListado';
import { formatearMoneda } from '../../format/formato';

export function Insumos() {
  const insumos = useListado(listarInsumos);
  const proveedores = useListado(listarProveedoresInsumo);
  const [nombre, setNombre] = useState('');
  const [proveedorId, setProveedorId] = useState<number | null>(null);
  const [precio, setPrecio] = useState('');

  const enviar = async () => {
    await crearInsumo({ nombre, proveedorInsumoId: proveedorId, precioUnitario: aNumero(precio) });
    setNombre('');
    setPrecio('');
    await insumos.recargar();
  };

  return (
    <section>
      <h1>Insumos</h1>
      <Formulario titulo="Nuevo insumo" enviar={enviar}>
        <Campo etiqueta="Nombre" nombre="nombre">
          <input value={nombre} onChange={(e) => setNombre(e.target.value)} />
        </Campo>
        <Campo etiqueta="Proveedor de insumos" nombre="proveedorInsumoId">
          <SelectorId opciones={proveedores.filas} valor={proveedorId} onChange={setProveedorId} />
        </Campo>
        <Campo etiqueta="Precio por unidad ($)" nombre="precioUnitario">
          <CampoKg valor={precio} onChange={setPrecio} decimales={2} />
        </Campo>
      </Formulario>
      <Tabla
        filas={insumos.filas}
        clave={(i) => i.id}
        columnas={[
          { titulo: 'Nombre', valor: (i) => i.nombre },
          { titulo: 'Proveedor', valor: (i) => i.proveedorInsumo.nombre },
          { titulo: 'Precio por unidad', valor: (i) => formatearMoneda(i.precioUnitario), numerica: true },
        ]}
      />
    </section>
  );
}
