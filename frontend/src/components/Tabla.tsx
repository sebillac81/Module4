import { ReactNode } from 'react';

export interface Columna<T> {
  titulo: string;
  valor: (fila: T) => ReactNode;
  numerica?: boolean;
}

interface Props<T> {
  columnas: Columna<T>[];
  filas: T[];
  clave: (fila: T) => string | number;
}

export function Tabla<T>({ columnas, filas, clave }: Props<T>) {
  if (filas.length === 0) {
    return <p className="vacio">No hay registros.</p>;
  }
  return (
    <div className="tabla-contenedor">
      <table>
        <thead>
          <tr>
            {columnas.map((c) => (
              <th key={c.titulo} className={c.numerica ? 'num' : undefined}>
                {c.titulo}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {filas.map((f) => (
            <tr key={clave(f)}>
              {columnas.map((c) => (
                <td key={c.titulo} className={c.numerica ? 'num' : undefined}>
                  {c.valor(f)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
