-- Esquema inicial de RoastingManager (data-model.md).
-- Cantidades de café en gramos enteros (bigint); montos numeric(12,2); porcentajes numeric(9,2).

CREATE TABLE proveedor_cafe (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre     varchar(120) NOT NULL CHECK (btrim(nombre) <> ''),
    telefono   varchar(40),
    email      varchar(160),
    direccion  varchar(200),
    creado_en  timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX ux_proveedor_cafe_nombre ON proveedor_cafe (lower(nombre));

CREATE TABLE proveedor_insumo (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre     varchar(120) NOT NULL CHECK (btrim(nombre) <> ''),
    telefono   varchar(40),
    email      varchar(160),
    direccion  varchar(200),
    creado_en  timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX ux_proveedor_insumo_nombre ON proveedor_insumo (lower(nombre));

CREATE TABLE cliente (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre     varchar(120) NOT NULL CHECK (btrim(nombre) <> ''),
    telefono   varchar(40),
    email      varchar(160),
    direccion  varchar(200),
    creado_en  timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX ux_cliente_nombre ON cliente (lower(nombre));

CREATE TABLE cafe_verde (
    id                 bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre             varchar(120) NOT NULL CHECK (btrim(nombre) <> ''),
    proveedor_cafe_id  bigint NOT NULL REFERENCES proveedor_cafe (id),
    precio_kg          numeric(12,2) NOT NULL CHECK (precio_kg >= 0.01),
    stock_inicial_g    bigint NOT NULL CHECK (stock_inicial_g >= 0),
    stock_verde_g      bigint NOT NULL CHECK (stock_verde_g >= 0),
    stock_tostado_g    bigint NOT NULL DEFAULT 0 CHECK (stock_tostado_g >= 0),
    costo_promedio_kg  numeric(12,2),
    creado_en          timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX ux_cafe_verde_nombre ON cafe_verde (lower(nombre));

CREATE TABLE insumo (
    id                   bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre               varchar(120) NOT NULL CHECK (btrim(nombre) <> ''),
    proveedor_insumo_id  bigint NOT NULL REFERENCES proveedor_insumo (id),
    precio_unitario      numeric(12,2) NOT NULL CHECK (precio_unitario >= 0.01),
    creado_en            timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX ux_insumo_nombre ON insumo (lower(nombre));

CREATE TABLE presentacion (
    id         bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre     varchar(120) NOT NULL CHECK (btrim(nombre) <> ''),
    gramos     integer NOT NULL CHECK (gramos > 0),
    creado_en  timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX ux_presentacion_nombre ON presentacion (lower(nombre));

CREATE TABLE presentacion_insumo (
    presentacion_id  bigint NOT NULL REFERENCES presentacion (id),
    insumo_id        bigint NOT NULL REFERENCES insumo (id),
    PRIMARY KEY (presentacion_id, insumo_id)
);

CREATE TABLE ingreso_stock (
    id             bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cafe_verde_id  bigint NOT NULL REFERENCES cafe_verde (id),
    cantidad_g     bigint NOT NULL CHECK (cantidad_g > 0),
    fecha          date NOT NULL,
    creado_en      timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE batch (
    id                  bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cafe_verde_id       bigint NOT NULL REFERENCES cafe_verde (id),
    verde_g             bigint NOT NULL CHECK (verde_g > 0),
    tostado_g           bigint NOT NULL CHECK (tostado_g > 0),
    fecha               date NOT NULL,
    humedad_pct         numeric(5,2) CHECK (humedad_pct BETWEEN 0 AND 100),
    densidad_gl         numeric(8,2) CHECK (densidad_gl > 0),
    desarrollo_pct      numeric(5,2) CHECK (desarrollo_pct BETWEEN 0 AND 100),
    precio_kg_aplicado  numeric(12,2) NOT NULL,
    merma_pct           numeric(9,2) NOT NULL,
    costo_tostado_kg    numeric(12,2) NOT NULL,
    costo_total         numeric(12,2) NOT NULL,
    creado_en           timestamptz NOT NULL DEFAULT now(),
    CHECK (tostado_g < verde_g)
);

CREATE TABLE venta (
    id                          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cliente_id                  bigint NOT NULL REFERENCES cliente (id),
    cafe_verde_id               bigint NOT NULL REFERENCES cafe_verde (id),
    presentacion_id             bigint NOT NULL REFERENCES presentacion (id),
    unidades                    integer NOT NULL CHECK (unidades > 0),
    precio_unitario             numeric(12,2) NOT NULL CHECK (precio_unitario >= 0.01),
    fecha                       date NOT NULL,
    gramos_presentacion         integer NOT NULL CHECK (gramos_presentacion > 0),
    gramos_vendidos             bigint NOT NULL CHECK (gramos_vendidos > 0),
    costo_promedio_kg_aplicado  numeric(12,2) NOT NULL,
    ingreso                     numeric(12,2) NOT NULL,
    costo_cafe_vendido          numeric(12,2) NOT NULL,
    costos_asociados            numeric(12,2) NOT NULL,
    ganancia                    numeric(12,2) NOT NULL,
    margen_pct                  numeric(9,2) NOT NULL,
    creado_en                   timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE venta_insumo (
    venta_id                  bigint NOT NULL REFERENCES venta (id),
    insumo_id                 bigint NOT NULL REFERENCES insumo (id),
    nombre_insumo             varchar(120) NOT NULL,
    precio_unitario_aplicado  numeric(12,2) NOT NULL,
    PRIMARY KEY (venta_id, insumo_id)
);
