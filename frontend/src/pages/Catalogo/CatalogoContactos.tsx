import { useState } from 'react';
import { api } from '../../api/client';
import { Campo, Formulario } from '../../components/Formulario';
import { Tabla } from '../../components/Tabla';
import { useListado } from '../../components/useListado';

export interface Contacto {
  id: number;
  nombre: string;
  telefono: string | null;
  email: string | null;
  direccion: string | null;
}

interface Props {
  titulo: string;
  /** Ruta de la API, p. ej. "/api/clientes". */
  ruta: string;
}

/** Alta y listado de proveedores de café, proveedores de insumos o clientes. */
export function CatalogoContactos({ titulo, ruta }: Props) {
  const { filas, error, recargar } = useListado(() => api.get<Contacto[]>(ruta));
  const [nombre, setNombre] = useState('');
  const [telefono, setTelefono] = useState('');
  const [email, setEmail] = useState('');
  const [direccion, setDireccion] = useState('');

  const enviar = async () => {
    await api.post(ruta, {
      nombre,
      telefono: telefono || null,
      email: email || null,
      direccion: direccion || null,
    });
    setNombre('');
    setTelefono('');
    setEmail('');
    setDireccion('');
    await recargar();
  };

  return (
    <section>
      <h1>{titulo}</h1>
      <Formulario titulo="Nuevo" enviar={enviar}>
        <Campo etiqueta="Nombre" nombre="nombre">
          <input value={nombre} onChange={(e) => setNombre(e.target.value)} />
        </Campo>
        <Campo etiqueta="Teléfono" nombre="telefono">
          <input value={telefono} onChange={(e) => setTelefono(e.target.value)} />
        </Campo>
        <Campo etiqueta="Email" nombre="email">
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        </Campo>
        <Campo etiqueta="Dirección" nombre="direccion">
          <input value={direccion} onChange={(e) => setDireccion(e.target.value)} />
        </Campo>
      </Formulario>
      {error && <p role="alert">{error}</p>}
      <Tabla
        filas={filas}
        clave={(f) => f.id}
        columnas={[
          { titulo: 'Nombre', valor: (f) => f.nombre },
          { titulo: 'Teléfono', valor: (f) => f.telefono ?? '' },
          { titulo: 'Email', valor: (f) => f.email ?? '' },
          { titulo: 'Dirección', valor: (f) => f.direccion ?? '' },
        ]}
      />
    </section>
  );
}
