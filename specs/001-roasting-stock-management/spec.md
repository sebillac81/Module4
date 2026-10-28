# Feature Specification: Gestión de Tuestes, Stock y Rentabilidad

**Feature Branch**: `001-roasting-stock-management`

**Created**: 2026-10-27

**Status**: Draft

**Input**: User description: "prd2.md" — PRD-001: RoastingManager — Gestión de Tuestes y Stock de Café

## Clarifications

### Session 2026-10-27

- Q: ¿Cómo entra stock de café verde si la gestión de compras está fuera de alcance? → A:
  Stock inicial al registrar el café verde + ingresos de stock simples (café, kg, fecha).
- Q: ¿El stock de café tostado es un pool global o por café? → A: Un pool por café verde
  de origen, con su propio costo promedio ponderado; la venta indica qué café se vende.
- Q: ¿Qué costo promedio ponderado usa el margen de una venta? → A: El vigente al
  registrar la venta, que queda fijo.
- Q: ¿Cuándo se redondea a 2 decimales? → A: Cada valor calculado se redondea al guardarse
  y los cálculos posteriores usan el valor redondeado.
- Q: ¿Con qué precisión se registran las cantidades de café? → A: Internamente en gramos
  enteros; en pantalla kg (café verde, batches, stock) o gramos (presentaciones) según el
  caso.
- Q: ¿Pueden repetirse los nombres en el catálogo? → A: No; el nombre es único dentro de
  cada tipo de entidad, sin distinguir mayúsculas ni espacios al inicio/final.
- Q: ¿Qué fechas se aceptan en batches, ingresos y ventas? → A: Por defecto hoy; se
  permiten fechas pasadas y se rechazan futuras; la fecha no afecta cálculos ni stock.
- Q: ¿Cómo se prueban los precios congelados si no hay edición de catálogo? → A: Los
  escenarios verifican que el batch/venta guarda y muestra los precios usados; la regla queda
  lista para cuando exista edición.
- Q: ¿Rentabilidad y margen son lo mismo? → A: La rentabilidad muestra la ganancia en $ y el
  margen en %.
- Q: ¿Con cuántos decimales se cargan los precios? → A: Todos con hasta 2 decimales, mínimo
  $0,01.
- Q: ¿Hay topes para prevenir errores de carga? → A: Sí, configurables, con valores por
  defecto (ingreso ≤ 1000 kg, batch ≤ 100 kg de verde, venta ≤ 1000 unidades).
- Q: ¿Qué rangos valen para los datos de proceso? → A: Rangos físicos: humedad y desarrollo
  0–100 %, densidad > 0 g/L.
- Q: ¿Se acepta un batch con merma 0 %? → A: No; el tostado debe ser menor al verde.
- Q: ¿Qué costo promedio muestra la consulta de stock si el tostado de un café está en 0?
  → A: El último costo promedio conocido; "—" si nunca se tostó.

### Session 2026-10-28

- Q: ¿Qué es el "costo de producción" de un batch? → A: Los dos valores: costo del tostado
  en $/kg y costo total del batch en $ (precio del verde × kg de verde), ambos guardados al
  registrar.
- Q: Si una operación tiene datos inválidos y además falta stock, ¿qué se informa? → A:
  Primero se validan los datos y se informan todos sus errores juntos; el stock se controla
  solo si los datos son válidos.
- Q: ¿El stock inicial de un café verde tiene tope? → A: Sí, el mismo que un ingreso de
  stock (por defecto ≤ 1000 kg).
- Q: ¿Qué pasa si un dato de proceso tiene más de 2 decimales? → A: Se rechaza, igual que
  precios y kg.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar un batch de tueste con stock y costo automáticos (Priority: P1)

El tostador registra un batch eligiendo un único café verde, la cantidad de café verde
utilizada, la cantidad de café tostado obtenida, la fecha y, opcionalmente, datos del
proceso (humedad, densidad, porcentaje de desarrollo). El sistema descuenta el café verde
del stock, suma el café tostado al stock, calcula la merma y el costo del café tostado, y
actualiza el costo promedio ponderado del stock de café tostado. Para que esto sea posible,
el administrador antes registra proveedores de café y cafés verdes con su precio por kg.

**Why this priority**: es el núcleo del producto: sin batches no hay stock de café tostado
ni costo de producción, que son la base de las ventas y la rentabilidad.

**Independent Test**: se registra un proveedor de café y un café verde con stock y precio,
se registra un batch, y se verifica el stock resultante de verde y tostado, la merma, el
costo del café tostado y el costo promedio ponderado.

**Acceptance Scenarios**:

1. **Given** un proveedor de café registrado, **When** el administrador registra un café
   verde con precio de compra por kg mayor a cero, **Then** el café verde queda registrado
   (AC-01, AC-03a).
2. **Given** un precio de compra por kg menor o igual a cero, **When** se intenta guardar el
   café verde, **Then** el sistema rechaza la operación e informa que el precio debe ser
   mayor a cero (AC-03b).
3. **Given** un café verde con 2,00 kg de stock, **When** se registra un ingreso de 8,00 kg,
   **Then** el stock pasa a 10,00 kg; un ingreso con cantidad ≤ 0 se rechaza (FR-007a).
4. **Given** 10,00 kg de stock de un café verde a $20,00/kg, **When** el tostador registra un
   batch con 5,00 kg de verde y 4,25 kg de tostado, **Then** el stock de verde queda en
   5,00 kg, el de tostado aumenta 4,25 kg, la merma es 15,00 %, el costo del tostado es
   $23,53/kg y el costo total del batch es $100,00 (AC-07a, AC-08, AC-09, AC-11, AC-12).
5. **Given** un stock de tostado de 4,00 kg con costo promedio $20,00/kg, **When** se registra
   un batch que agrega 2,00 kg a $26,00/kg, **Then** el costo promedio ponderado pasa a
   $22,00/kg (AC-13).
6. **Given** un stock de café verde de 3,00 kg, **When** se intenta registrar un batch con
   5,00 kg de verde, **Then** el sistema impide la operación, informa que no hay stock
   suficiente y ningún stock cambia (AC-10).
7. **Given** una cantidad de verde o de tostado menor o igual a cero, **When** se intenta
   registrar el batch, **Then** el sistema rechaza la operación e informa que las cantidades
   deben ser mayores a cero (AC-07b).
8. **Given** un batch registrado, **When** se consulta, **Then** muestra el precio por kg del
   café verde usado en su cálculo, guardado al registrarlo; el costo del batch no depende
   de valores leídos después del registro (RF-12).

---

### User Story 2 - Registrar una venta con costos y margen automáticos (Priority: P2)

El administrador o el tostador registra una venta indicando cliente, presentación de
venta, cantidad de unidades, precio de venta y fecha. El sistema descuenta del stock de
café tostado los gramos correspondientes, calcula los costos asociados (insumos de la
presentación) y el margen de ganancia. Para ello, previamente se registran proveedores de
insumos, insumos con precio unitario, presentaciones (gramos por unidad + insumos) y
clientes.

**Why this priority**: permite conocer la rentabilidad real de cada venta, el segundo
objetivo central del producto; depende de que exista stock de café tostado (US1).

**Independent Test**: con stock de café tostado disponible, se registran insumos, una
presentación, un cliente y una venta, y se verifica el descuento de stock, los costos
asociados y el margen.

**Acceptance Scenarios**:

1. **Given** datos válidos, **When** se registran un proveedor de insumos, un insumo con
   precio unitario mayor a cero, una presentación con gramos mayores a cero y cero o más
   insumos, y un cliente, **Then** todos quedan registrados (AC-02, AC-04a, AC-05a, AC-06).
2. **Given** un precio unitario de insumo o gramos de presentación menores o iguales a cero,
   **When** se intenta guardar, **Then** el sistema rechaza la operación con el mensaje
   correspondiente (AC-04b, AC-05b).
3. **Given** 5,00 kg de stock de tostado del café "Colombia" con costo promedio $22,00/kg y
   una presentación de 250 g con bolsa ($0,50) y etiqueta ($0,10), **When** se registra la
   venta de 4 unidades de "Colombia" a $15,00 c/u, **Then** el stock de tostado de
   "Colombia" baja 1,00 kg (el de otros cafés no cambia), el ingreso es
   $60,00, el costo del café vendido $22,00, los costos asociados $2,40, la ganancia $35,60
   y el margen (35,60 / 60,00) × 100 = 59,33 % (AC-14a, AC-15, AC-17, AC-18).
4. **Given** un stock de tostado menor a los gramos a vender, **When** se intenta registrar
   la venta, **Then** el sistema la impide, informa que no hay stock suficiente y ningún
   stock cambia (AC-16).
5. **Given** cantidad de unidades o precio de venta menor o igual a cero, **When** se intenta
   registrar la venta, **Then** el sistema la rechaza e informa que ambos deben ser mayores
   a cero (AC-14b).
6. **Given** una venta registrada, **When** se consulta, **Then** muestra los precios de
   insumos y el costo promedio ponderado usados en su cálculo, guardados al registrarla;
   **And** si después se registra un nuevo batch del mismo café, su costo, ganancia y margen
   no cambian (RF-17, FR-018).

---

### User Story 3 - Consultar stock, historiales, costos y rentabilidad (Priority: P3)

El administrador consulta el stock disponible de cada café, el historial de batches y de
ventas, el costo de producción de cada batch, y los costos asociados y la rentabilidad de
cada venta.

**Why this priority**: da visibilidad para analizar el negocio; aporta valor una vez que
existen batches y ventas.

**Independent Test**: con batches y ventas cargados, cada consulta muestra los valores
calculados al momento del registro.

**Acceptance Scenarios**:

1. **Given** cafés con stock, **When** el usuario consulta el stock, **Then** ve el stock de
   café verde y de café tostado disponible de cada café (AC-19).
2. **Given** batches registrados, **When** consulta el historial, **Then** ve el listado de
   batches con fecha, café, cantidades, merma y costo de producción: costo del tostado en
   $/kg y costo total en $ (AC-20, AC-22).
3. **Given** ventas registradas, **When** consulta el historial, **Then** ve el listado de
   ventas con cliente, presentación, unidades, precio, costos asociados y margen (AC-21,
   AC-23, AC-24).
4. **Given** que no hay registros, **When** consulta cualquier listado, **Then** el sistema
   muestra un listado vacío con un mensaje indicándolo.

---

### Edge Cases

- Batch que consume exactamente todo el stock de café verde: se permite y el stock queda
  en 0.
- Venta que consume exactamente todo el stock de café tostado: se permite y el stock queda
  en 0.
- Cantidad de café tostado obtenida mayor o igual que la de verde utilizado (merma ≤ 0 %):
  se rechaza, ya que el tueste siempre reduce el peso.
- Batch posterior a haber vendido todo el stock de tostado de un café (stock 0): el costo
  promedio ponderado pasa a ser el costo del nuevo batch (FR-013 con stock existente 0).
- Ventas sucesivas que dejan el stock de tostado exactamente en 0: se aceptan; la venta
  siguiente de ese café se rechaza por stock insuficiente.
- Cantidades mínimas (1 g): se aceptan; merma y costo se calculan y redondean igual que en
  cualquier otro caso (FR-024).
- Cantidad que supera el tope configurado (FR-030): se rechaza informando el tope.
- Datos inválidos y stock insuficiente a la vez (p. ej. batch de 150 kg sobre 5 kg de
  stock): se informan solo los errores de datos (FR-025).
- Primer batch de un café sin stock de tostado previo: el costo promedio ponderado de ese
  café pasa a ser el costo del batch.
- Café verde registrado con stock inicial 0: se permite; no podrá tostarse hasta registrar
  un ingreso de stock.
- Venta de un café que nunca fue tostado: se rechaza por stock insuficiente.
- Presentación sin insumos: los costos asociados de la venta son $0,00.
- Precio de venta tan bajo que el margen resulta negativo: se registra igual y el margen
  se muestra negativo.
- Dos usuarios registran a la vez operaciones que, juntas, superan el stock: solo se
  acepta la que entra en el stock disponible; la otra se rechaza por stock insuficiente.
- Datos del proceso opcionales vacíos: el batch se registra igual.
- Batch, ingreso o venta con fecha futura: se rechaza. Con fecha pasada (carga atrasada):
  se acepta y se calcula contra el stock y el costo promedio actuales.

## Requirements *(mandatory)*

### Functional Requirements

**Catálogo**

- **FR-001**: El sistema MUST permitir registrar y consultar proveedores de café (RF-01).
- **FR-002**: El sistema MUST permitir registrar y consultar proveedores de insumos (RF-02).
- **FR-003**: El sistema MUST permitir registrar y consultar cafés verdes, asociados a un
  proveedor de café, con precio de compra por kg mayor a cero (RF-03).
- **FR-004**: El sistema MUST permitir registrar y consultar insumos, asociados a un
  proveedor de insumos, con precio por unidad mayor a cero (RF-04).
- **FR-005**: El sistema MUST permitir registrar y consultar presentaciones de venta con
  gramos de café tostado por unidad mayores a cero y cero o más insumos asociados (RF-05).
- **FR-006**: El sistema MUST permitir registrar y consultar clientes (RF-06).
- **FR-007**: El sistema MUST permitir indicar un stock inicial de café verde en kg (≥ 0 y
  ≤ tope de ingreso de stock, FR-030) al registrar el café verde (RF-03).
- **FR-007a**: El sistema MUST permitir registrar ingresos de stock de café verde indicando
  café verde, cantidad en kg (> 0) y fecha; cada ingreso incrementa el stock de ese café.
  El ingreso no registra proveedor, precio ni orden de compra (la gestión de compras sigue
  fuera de alcance) y no modifica el precio por kg del catálogo (RF-25).
- **FR-007b**: El sistema MUST permitir consultar el historial de ingresos de stock de cada
  café verde.

**Batches**

- **FR-008**: El sistema MUST permitir registrar un batch con un único café verde, cantidad
  de verde utilizada (> 0), cantidad de tostado obtenida (> 0 y estrictamente menor a la de
  verde), fecha y datos de proceso opcionales (RF-07). Si se informan, los datos de proceso
  MUST respetar rangos físicos, con hasta 2 decimales: humedad 0–100 %, densidad > 0 g/L,
  porcentaje de desarrollo 0–100 %. Un dato de proceso con más de 2 decimales MUST
  rechazarse (no se redondea).
- **FR-009**: Al registrar un batch, el sistema MUST descontar la cantidad de verde del
  stock de ese café verde e incrementar el stock de café tostado **de ese mismo café** en la
  cantidad obtenida (RF-08, RF-09).
- **FR-010**: El sistema MUST impedir registrar un batch si el stock del café verde es menor
  a la cantidad a utilizar, informando la falta de stock junto con el stock disponible y la
  cantidad requerida, en kg (RF-10).
- **FR-011**: El sistema MUST calcular la merma como ((verde − tostado) / verde) × 100,
  sobre las cantidades en gramos, expresada en % (RF-11).
- **FR-012**: El sistema MUST calcular el costo del tostado, expresado en $/kg, como
  (precio por kg del verde × gramos de verde) / gramos de tostado, guardando de forma
  permanente en el batch el precio del verde vigente al registrarlo (RF-12). En esta
  versión, sin edición de catálogo, es el precio con el que se dio de alta el café verde;
  guardarlo en el batch deja la regla lista para cuando exista la edición.
- **FR-012a**: Al registrar un batch, el sistema MUST calcular y guardar su costo total en $
  como precio por kg del verde × gramos de verde / 1000, redondeado según FR-024 (p. ej.
  $20,00/kg × 5,000 kg = $100,00). El costo de producción de un batch se compone del costo
  del tostado en $/kg (FR-012) y de este costo total.
- **FR-013**: Al registrar un batch, el sistema MUST recalcular el costo promedio ponderado
  del stock de café tostado de ese café, expresado en $/kg, como ((stock existente × costo
  promedio actual) + (cantidad del batch × costo del batch)) / (stock existente + cantidad
  del batch), donde "stock existente" es el stock de tostado de ese mismo café antes de
  sumar el batch (RF-13).

**Ventas**

- **FR-014**: El sistema MUST permitir registrar una venta con cliente, café vendido,
  presentación, unidades (> 0), precio de venta por unidad (> 0) y fecha (RF-14).
- **FR-015**: El stock de café tostado MUST llevarse como un pool por café verde de origen,
  con su propio costo promedio ponderado. Al registrar una venta, el sistema MUST descontar
  del stock de tostado del café vendido unidades × gramos de la presentación (RF-15). Una
  venta MUST NOT modificar el costo promedio ponderado: solo lo recalculan los batches.
- **FR-016**: El sistema MUST impedir registrar una venta si el stock de tostado del café
  vendido es menor a los gramos a vender, informando la falta de stock junto con el stock
  disponible y la cantidad requerida, en kg (RF-16).
- **FR-017**: El sistema MUST calcular los costos asociados, en $ totales de la venta, como
  unidades × suma de los
  precios unitarios de los insumos de la presentación, guardando de forma permanente en la
  venta los precios de insumos vigentes al registrarla (RF-17).
- **FR-018**: Al registrar una venta, el sistema MUST calcular y guardar, en este orden y
  cada uno redondeado según FR-024 (RF-18):
  - ingreso = precio por unidad × unidades;
  - costo del café vendido = costo promedio ponderado del café vendido vigente al
    registrar la venta × kg de tostado vendidos;
  - ganancia ($) = ingreso − (costo del café vendido + costos asociados);
  - margen de ganancia (%) = ganancia / ingreso × 100.
  El costo promedio ponderado usado se guarda en la venta; ninguno de estos valores cambia
  con batches posteriores.

**Consultas**

- **FR-019**: El sistema MUST permitir consultar, para cada café, el stock disponible de
  café verde, el de café tostado y su costo promedio ponderado (RF-19). Si el stock de
  tostado es 0, se muestra el último costo promedio ponderado conocido como referencia; si
  el café nunca fue tostado, se muestra "—".
- **FR-020**: El sistema MUST permitir consultar el historial de batches, incluyendo su costo
  de producción: costo del tostado en $/kg y costo total en $ (FR-012a; RF-20, RF-22).
- **FR-021**: El sistema MUST permitir consultar el historial de ventas, incluyendo costos
  asociados y rentabilidad de cada una (RF-21, RF-23, RF-24). La rentabilidad de una venta
  se compone de la ganancia en $ y el margen de ganancia en % (FR-018).

**Reglas transversales**

- **FR-022**: El stock de café verde y de café tostado MUST NOT quedar nunca por debajo de
  0 (RNF-02), tampoco ante operaciones simultáneas: si dos operaciones concurrentes sobre el
  mismo stock lo superarían juntas, solo se acepta la que entra en el stock disponible y la
  otra se rechaza por stock insuficiente.
- **FR-023**: El registro de un batch, un ingreso de stock o una venta MUST aplicarse completo (registro,
  movimientos de stock y cálculos) o no aplicarse en absoluto.
- **FR-024**: Los valores monetarios (costo del tostado, costo total del batch, costo
  promedio ponderado, costos asociados) y porcentuales (merma, margen) MUST expresarse con 2 decimales (RNF-03).
  Cada valor se redondea una única vez, al guardarse, con redondeo comercial: la mitad se
  aleja de cero (2,345 → 2,35; 2,344 → 2,34; −2,345 → −2,35);
  los cálculos posteriores (p. ej. costo promedio ponderado a partir del costo del batch, o
  margen a partir de costos asociados) MUST usar el valor ya redondeado, de modo que todo
  resultado sea reproducible a mano con los valores mostrados.
- **FR-026**: Todas las cantidades de café (stock verde y tostado, batches, ingresos,
  presentaciones, descuentos por venta) MUST registrarse internamente en gramos enteros.
  En pantalla se cargan y muestran en la unidad natural de cada caso: kg con hasta 3
  decimales (p. ej. una bolsa de 69 kg, un batch de 5,250 kg) para café verde, batches y
  stock; gramos enteros para presentaciones. La conversión es exacta (1 kg = 1000 g; un kg
  con 3 decimales equivale a gramos enteros), por lo que un valor en kg con más de 3
  decimales MUST rechazarse. Los precios de café verde se mantienen por kg; el costo se calcula como
  precio por kg × gramos / 1000.
- **FR-027**: El nombre de proveedores de café, proveedores de insumos, cafés verdes,
  insumos, presentaciones y clientes MUST ser obligatorio y único dentro de su tipo. La
  comparación ignora mayúsculas/minúsculas y espacios al inicio y al final; un duplicado
  MUST rechazarse informando que ya existe un registro con ese nombre.
- **FR-028**: La fecha de batches, ingresos de stock y ventas MUST tomar por defecto el día
  actual, admitir fechas pasadas y rechazar fechas futuras. La fecha es informativa: el
  stock, el costo promedio ponderado y los demás cálculos se aplican según el orden de
  registro, no según la fecha. Los historiales se ordenan por fecha descendente.
- **FR-025**: Ante datos inválidos, el sistema MUST rechazar la operación sin guardar nada
  e informar un mensaje claro que indique el campo y la regla incumplida. Las validaciones
  de datos (obligatorios, > 0, decimales, rangos, fechas, topes, referencias existentes)
  MUST evaluarse antes que el stock y reportar juntos todos los errores de datos; el stock
  suficiente (FR-010, FR-016) se controla solo si los datos son válidos y se informa como
  un error distinto.
- **FR-029**: Los montos que carga el usuario (precio por kg del café verde, precio por
  unidad del insumo, precio de venta por unidad) MUST tener como máximo 2 decimales y ser
  ≥ 0,01. Un insumo de menor valor se carga agrupado (p. ej. por pack) o redondeado.
- **FR-030**: El sistema MUST aplicar topes máximos por operación para prevenir errores de
  carga, configurables sin modificar el sistema y con estos valores por defecto: ingreso
  de stock de café verde y stock inicial de un café verde ≤ 1000 kg, café verde por batch ≤ 100 kg, unidades por venta
  ≤ 1000. Un valor que supere el tope MUST rechazarse informando el tope vigente.
- **FR-031**: El stock disponible MUST ser siempre reconstruible a partir de los registros:
  stock verde = stock inicial + ingresos − verde usado en batches; stock tostado = tostado
  obtenido en batches − gramos vendidos.

### Key Entities *(include if feature involves data)*

- **Proveedor de café**: quien vende café verde. Nombre y datos de contacto.
- **Proveedor de insumos**: quien vende insumos de empaque. Nombre y datos de contacto.
- **Café verde**: café sin tostar. Nombre/origen, proveedor de café, precio de compra por
  kg vigente, stock inicial y stock disponible (se muestran en kg).
- **Ingreso de stock de café verde**: café verde, cantidad en kg y fecha.
- **Insumo**: material de empaque (bolsa, etiqueta). Nombre, proveedor de insumos, precio
  por unidad vigente.
- **Presentación de venta**: forma en que se vende el tostado. Nombre, gramos por unidad,
  lista de insumos asociados (0..n).
- **Cliente**: comprador de café tostado. Nombre y datos de contacto.
- **Batch de tueste**: café verde usado, kg de verde, kg de tostado, fecha, datos de proceso
  opcionales, precio del verde congelado, merma, costo del tostado ($/kg) y costo total ($)
  calculados.
- **Stock de café tostado**: uno por café verde de origen; cantidad disponible (se muestra
  en kg) y costo promedio ponderado en $/kg.
- **Venta**: cliente, café vendido, presentación, unidades, precio por unidad, fecha,
  precios de insumos y costo promedio ponderado congelados; ingreso, costo del café
  vendido, costos asociados, ganancia y margen calculados.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El tostador registra un batch completo en menos de 1 minuto.
- **SC-002**: Un usuario registra una venta completa en menos de 1 minuto.
- **SC-003**: El 100 % de los batches y ventas muestra merma, costo, costos asociados y
  margen calculados automáticamente, sin cálculos manuales del usuario.
- **SC-004**: En ningún momento el stock de café verde o tostado consultado es negativo,
  incluso cuando 5 usuarios registran a la vez batches o ventas sobre el mismo café con
  cantidades que, sumadas, superan su stock.
- **SC-005**: El 95 % de las operaciones principales (registrar y consultar) se completa en
  menos de 3 segundos con hasta 5 usuarios concurrentes (RNF-01).
- **SC-006**: Los valores calculados coinciden al centavo con un cálculo manual de las
  fórmulas del PRD en el 100 % de los casos de prueba.
- **SC-007**: El administrador conoce el stock disponible y la rentabilidad de cualquier
  venta en menos de 30 segundos, sin planillas externas.

## Assumptions

- Los ingresos de stock de café verde (FR-007a) son un movimiento de stock mínimo, no una
  compra: no reemplazan ni anticipan la gestión de compras excluida por el PRD. Esta
  decisión quedó incorporada al PRD como RF-25 / AC-25a-b, junto con el stock de tostado
  por café (RF-09, RF-13 a RF-16, RF-19).
- Unidad de carga por pantalla (FR-026): café verde (stock inicial e ingresos), batches y
  consulta de stock en kg; presentaciones en gramos. Los batches se cargan en kg porque los
  pesos de tueste son del orden de kilos (p. ej. 5,250 kg).
- El precio de venta de una venta es por unidad de presentación.
- Cada café verde pertenece a un proveedor de café y cada insumo a un proveedor de insumos.
- Los datos de contacto de proveedores y clientes (nombre obligatorio; teléfono, email y
  dirección opcionales) son suficientes para esta versión.
- No se valida stock de insumos (bolsas, etiquetas): solo se usa su precio para el costo.
- Un batch con más tostado que verde es un error de carga y se rechaza.
- Los precios se manejan en una única moneda.
- No hay login ni control de acceso por rol: administrador y tostador ven y operan todo
  (Fuera de Alcance del PRD).
- Sin edición ni eliminación de catálogos, batches ni ventas en esta versión. Se prevé
  agregarlas en una versión futura; la unicidad de nombres (FR-027) compensa mientras tanto
  la imposibilidad de corregir errores de carga.
- Fuera de alcance: CRM, facturación, e-commerce, gestión de compras, multitenancy, FIFO
  por lote, blends y análisis de curvas de Artisan.
