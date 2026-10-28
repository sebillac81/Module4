-- Índices para los historiales (fecha descendente y, a igual fecha, orden de registro).
CREATE INDEX ix_batch_fecha ON batch (fecha DESC, id DESC);
CREATE INDEX ix_venta_fecha ON venta (fecha DESC, id DESC);
CREATE INDEX ix_ingreso_stock_cafe_fecha ON ingreso_stock (cafe_verde_id, fecha DESC, id DESC);
