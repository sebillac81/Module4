import { NavLink, Route, Routes } from 'react-router-dom';
import { Batches } from './pages/Batches';
import { CafesVerdes } from './pages/Catalogo/CafesVerdes';
import { ProveedoresCafe } from './pages/Catalogo/ProveedoresCafe';
import { Clientes } from './pages/Catalogo/Clientes';
import { Insumos } from './pages/Catalogo/Insumos';
import { Presentaciones } from './pages/Catalogo/Presentaciones';
import { ProveedoresInsumo } from './pages/Catalogo/ProveedoresInsumo';
import { HistorialBatches } from './pages/HistorialBatches';
import { HistorialVentas } from './pages/HistorialVentas';
import { Ingresos } from './pages/Ingresos';
import { Stock } from './pages/Stock';
import { Ventas } from './pages/Ventas';

interface Pagina {
  ruta: string;
  titulo: string;
  elemento: JSX.Element;
}

const paginas: Pagina[] = [
  // US3
  { ruta: '/stock', titulo: 'Stock', elemento: <Stock /> },
  { ruta: '/historial-batches', titulo: 'Historial de batches', elemento: <HistorialBatches /> },
  { ruta: '/historial-ventas', titulo: 'Historial de ventas', elemento: <HistorialVentas /> },
  // US1
  { ruta: '/batches', titulo: 'Batches', elemento: <Batches /> },
  { ruta: '/ingresos', titulo: 'Ingresos de verde', elemento: <Ingresos /> },
  { ruta: '/cafes-verdes', titulo: 'Cafés verdes', elemento: <CafesVerdes /> },
  { ruta: '/proveedores-cafe', titulo: 'Proveedores de café', elemento: <ProveedoresCafe /> },
  // US2
  { ruta: '/ventas', titulo: 'Ventas', elemento: <Ventas /> },
  { ruta: '/clientes', titulo: 'Clientes', elemento: <Clientes /> },
  { ruta: '/presentaciones', titulo: 'Presentaciones', elemento: <Presentaciones /> },
  { ruta: '/insumos', titulo: 'Insumos', elemento: <Insumos /> },
  { ruta: '/proveedores-insumo', titulo: 'Proveedores de insumos', elemento: <ProveedoresInsumo /> },
];

export function App() {
  return (
    <div className="app">
      <nav>
        <strong>RoastingManager</strong>
        {paginas.map((p) => (
          <NavLink key={p.ruta} to={p.ruta}>
            {p.titulo}
          </NavLink>
        ))}
      </nav>
      <main>
        <Routes>
          <Route path="/" element={<Stock />} />
          {paginas.map((p) => (
            <Route key={p.ruta} path={p.ruta} element={p.elemento} />
          ))}
        </Routes>
      </main>
    </div>
  );
}
