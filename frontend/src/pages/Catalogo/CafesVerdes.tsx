import { useState } from 'react';
import { crearCafeVerde, listarCafesVerdes, listarProveedoresCafe } from '../../api/cafe';
import { aNumero, CampoKg } from '../../components/CampoKg';
import { Campo, Formulario } from '../../components/Formulario';
import { SelectorId } from '../../components/SelectorId';
import { Tabla } from '../../components/Tabla';
import { useListado } from '../../components/useListado';
import { formatearKg, formatearMonedaKg } from '../../format/formato';

export function CafesVerdes() {
  const cafes = useListado(listarCafesVerdes);
  const proveedores = useListado(listarProveedoresCafe);
  const [nombre, setNombre] = useState('');
  const [proveedorId, setProveedorId] = useState<number | null>(null);
  const [precio, setPrecio] = useState('');
  const [stockInicial, setStockInicial] = useState('0');

  const enviar = async () => {
    await crearCafeVerde({
      nombre,
      proveedorCafeId: proveedorId,
      precioKg: aNumero(precio),
      stockInicialKg: aNumero(stockInicial),
    });
    setNombre('');
    setPrecio('');
    setStockInicial('0');
    await cafes.recargar();
  };

  return (
    <section>
      <h1>Cafés verdes</h1>
      <Formulario titulo="Nuevo café verde" enviar={enviar}>
        <Campo etiqueta="Nombre / origen" nombre="nombre">
          <input value={nombre} onChange={(e) => setNombre(e.target.value)} />
        </Campo>
        <Campo etiqueta="Proveedor de café" nombre="proveedorCafeId">
          <SelectorId opciones={proveedores.filas} valor={proveedorId} onChange={setProveedorId} />
        </Campo>
        <Campo etiqueta="Precio por kg ($)" nombre="precioKg">
          <CampoKg valor={precio} onChange={setPrecio} decimales={2} />
        </Campo>
        <Campo etiqueta="Stock inicial (kg)" nombre="stockInicialKg">
          <CampoKg valor={stockInicial} onChange={setStockInicial} />
        </Campo>
      </Formulario>
      <Tabla
        filas={cafes.filas}
        clave={(c) => c.id}
        columnas={[
          { titulo: 'Nombre', valor: (c) => c.nombre },
          { titulo: 'Proveedor', valor: (c) => c.proveedorCafe.nombre },
          { titulo: 'Precio', valor: (c) => formatearMonedaKg(c.precioKg), numerica: true },
          { titulo: 'Stock inicial', valor: (c) => formatearKg(c.stockInicialKg), numerica: true },
        ]}
      />
    </section>
  );
}
