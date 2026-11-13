/* =====================================================================
   PLAYERAS JyC - Base de datos de la tienda de playeras
   SQL Server (Express/Developer/Azure SQL)  -  18 tablas

   Cómo usarlo:
     1) Abre este archivo en SQL Server Management Studio (SSMS).
     2) Pulsa Ejecutar (F5). Crea la base PlayerasJyCDB, las tablas y los datos de ejemplo.
     3) Se puede volver a ejecutar: borra y recrea las tablas.

   Cuentas de ejemplo:
     Administrador : admin@playerasjyc.com    / Admin123
     Cliente       : cliente@playerasjyc.com  / Cliente123
   ===================================================================== */

USE master;
GO
IF DB_ID(N'PlayerasJyCDB') IS NULL
    CREATE DATABASE PlayerasJyCDB;
GO
USE PlayerasJyCDB;
GO

/* ---------- Borrar tablas si ya existen (orden inverso a las llaves foráneas) ---------- */

IF OBJECT_ID(N'dbo.Resenas', N'U') IS NOT NULL DROP TABLE dbo.Resenas;
IF OBJECT_ID(N'dbo.HistorialEstados', N'U') IS NOT NULL DROP TABLE dbo.HistorialEstados;
IF OBJECT_ID(N'dbo.Pagos', N'U') IS NOT NULL DROP TABLE dbo.Pagos;
IF OBJECT_ID(N'dbo.DetallePedidos', N'U') IS NOT NULL DROP TABLE dbo.DetallePedidos;
IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NOT NULL DROP TABLE dbo.Pedidos;
IF OBJECT_ID(N'dbo.CarritoItems', N'U') IS NOT NULL DROP TABLE dbo.CarritoItems;
IF OBJECT_ID(N'dbo.Variantes', N'U') IS NOT NULL DROP TABLE dbo.Variantes;
IF OBJECT_ID(N'dbo.ImagenesProducto', N'U') IS NOT NULL DROP TABLE dbo.ImagenesProducto;
IF OBJECT_ID(N'dbo.Productos', N'U') IS NOT NULL DROP TABLE dbo.Productos;
IF OBJECT_ID(N'dbo.Direcciones', N'U') IS NOT NULL DROP TABLE dbo.Direcciones;
IF OBJECT_ID(N'dbo.Cupones', N'U') IS NOT NULL DROP TABLE dbo.Cupones;
IF OBJECT_ID(N'dbo.MetodosEnvio', N'U') IS NOT NULL DROP TABLE dbo.MetodosEnvio;
IF OBJECT_ID(N'dbo.Colores', N'U') IS NOT NULL DROP TABLE dbo.Colores;
IF OBJECT_ID(N'dbo.Tallas', N'U') IS NOT NULL DROP TABLE dbo.Tallas;
IF OBJECT_ID(N'dbo.Colecciones', N'U') IS NOT NULL DROP TABLE dbo.Colecciones;
IF OBJECT_ID(N'dbo.Categorias', N'U') IS NOT NULL DROP TABLE dbo.Categorias;
IF OBJECT_ID(N'dbo.SolicitudesContacto', N'U') IS NOT NULL DROP TABLE dbo.SolicitudesContacto;
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL DROP TABLE dbo.Usuarios;
GO

/* ============================== TABLAS ============================== */

-- 1. Usuarios: clientes y administradores (Rol)
CREATE TABLE dbo.Usuarios (
    UsuarioId     INT IDENTITY(1,1) CONSTRAINT PK_Usuarios PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(100) NOT NULL CONSTRAINT UQ_Usuarios_Email UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,
    Telefono      NVARCHAR(20)  NULL,
    Rol           NVARCHAR(20)  NOT NULL CONSTRAINT DF_Usuarios_Rol DEFAULT N'Cliente',
    Activo        BIT           NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT 1,
    FechaRegistro DATETIME2     NOT NULL CONSTRAINT DF_Usuarios_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Cliente', N'Admin'))
);
GO

-- 2. Direcciones de envío guardadas por cada cliente
CREATE TABLE dbo.Direcciones (
    DireccionId        INT IDENTITY(1,1) CONSTRAINT PK_Direcciones PRIMARY KEY,
    UsuarioId          INT NOT NULL,
    Alias              NVARCHAR(50)  NOT NULL CONSTRAINT DF_Direcciones_Alias DEFAULT N'Casa',
    NombreDestinatario NVARCHAR(100) NOT NULL,
    Calle              NVARCHAR(150) NOT NULL,
    Colonia            NVARCHAR(100) NULL,
    Ciudad             NVARCHAR(100) NOT NULL,
    Estado             NVARCHAR(100) NOT NULL,
    CodigoPostal       NVARCHAR(10)  NOT NULL,
    Telefono           NVARCHAR(20)  NULL,
    Predeterminada     BIT NOT NULL CONSTRAINT DF_Direcciones_Pred DEFAULT 0,
    CONSTRAINT FK_Direcciones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(UsuarioId) ON DELETE CASCADE
);
GO

-- 3. Categorías de playeras
CREATE TABLE dbo.Categorias (
    CategoriaId INT IDENTITY(1,1) CONSTRAINT PK_Categorias PRIMARY KEY,
    Nombre      NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Categorias_Nombre UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    Activo      BIT NOT NULL CONSTRAINT DF_Categorias_Activo DEFAULT 1
);
GO

-- 4. Colecciones / temporadas
CREATE TABLE dbo.Colecciones (
    ColeccionId INT IDENTITY(1,1) CONSTRAINT PK_Colecciones PRIMARY KEY,
    Nombre      NVARCHAR(80)  NOT NULL CONSTRAINT UQ_Colecciones_Nombre UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    Activo      BIT NOT NULL CONSTRAINT DF_Colecciones_Activo DEFAULT 1
);
GO

-- 5. Tallas
CREATE TABLE dbo.Tallas (
    TallaId INT IDENTITY(1,1) CONSTRAINT PK_Tallas PRIMARY KEY,
    Nombre  NVARCHAR(10) NOT NULL CONSTRAINT UQ_Tallas_Nombre UNIQUE,
    Orden   INT NOT NULL CONSTRAINT DF_Tallas_Orden DEFAULT 0
);
GO

-- 6. Colores
CREATE TABLE dbo.Colores (
    ColorId   INT IDENTITY(1,1) CONSTRAINT PK_Colores PRIMARY KEY,
    Nombre    NVARCHAR(40) NOT NULL CONSTRAINT UQ_Colores_Nombre UNIQUE,
    CodigoHex NVARCHAR(7)  NOT NULL CONSTRAINT DF_Colores_Hex DEFAULT N'#000000'
);
GO

-- 7. Productos (catálogo)
CREATE TABLE dbo.Productos (
    ProductoId     INT IDENTITY(1,1) CONSTRAINT PK_Productos PRIMARY KEY,
    CategoriaId    INT NOT NULL,
    ColeccionId    INT NULL,
    Nombre         NVARCHAR(100) NOT NULL,
    Descripcion    NVARCHAR(MAX) NULL,
    Material       NVARCHAR(100) NULL,
    PrecioOriginal DECIMAL(18,2) NOT NULL,
    EnOferta       BIT NOT NULL CONSTRAINT DF_Productos_Oferta DEFAULT 0,
    PrecioOferta   DECIMAL(18,2) NULL,
    ImagenUrl      NVARCHAR(300) NULL,
    Activo         BIT NOT NULL CONSTRAINT DF_Productos_Activo DEFAULT 1,
    FechaAlta      DATETIME2 NOT NULL CONSTRAINT DF_Productos_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Productos_Precio CHECK (PrecioOriginal > 0),
    CONSTRAINT CK_Productos_Oferta CHECK (EnOferta = 0 OR (PrecioOferta IS NOT NULL AND PrecioOferta > 0 AND PrecioOferta < PrecioOriginal)),
    CONSTRAINT FK_Productos_Categorias  FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias(CategoriaId),
    CONSTRAINT FK_Productos_Colecciones FOREIGN KEY (ColeccionId) REFERENCES dbo.Colecciones(ColeccionId) ON DELETE SET NULL
);
GO

-- 8. Galería de imágenes de cada producto
CREATE TABLE dbo.ImagenesProducto (
    ImagenProductoId INT IDENTITY(1,1) CONSTRAINT PK_ImagenesProducto PRIMARY KEY,
    ProductoId       INT NOT NULL,
    Url              NVARCHAR(300) NOT NULL,
    Orden            INT NOT NULL CONSTRAINT DF_ImagenesProducto_Orden DEFAULT 0,
    CONSTRAINT FK_ImagenesProducto_Productos FOREIGN KEY (ProductoId) REFERENCES dbo.Productos(ProductoId) ON DELETE CASCADE
);
GO

-- 9. Variantes: existencias por producto + talla + color (inventario)
CREATE TABLE dbo.Variantes (
    VarianteId INT IDENTITY(1,1) CONSTRAINT PK_Variantes PRIMARY KEY,
    ProductoId INT NOT NULL,
    TallaId    INT NOT NULL,
    ColorId    INT NOT NULL,
    Sku        NVARCHAR(40) NULL,
    Stock      INT NOT NULL CONSTRAINT DF_Variantes_Stock DEFAULT 0,
    CONSTRAINT CK_Variantes_Stock CHECK (Stock >= 0),
    CONSTRAINT UQ_Variantes_Combinacion UNIQUE (ProductoId, TallaId, ColorId),
    CONSTRAINT FK_Variantes_Productos FOREIGN KEY (ProductoId) REFERENCES dbo.Productos(ProductoId) ON DELETE CASCADE,
    CONSTRAINT FK_Variantes_Tallas    FOREIGN KEY (TallaId)    REFERENCES dbo.Tallas(TallaId),
    CONSTRAINT FK_Variantes_Colores   FOREIGN KEY (ColorId)    REFERENCES dbo.Colores(ColorId)
);
GO

-- 10. Carrito de compras (una fila por variante agregada)
CREATE TABLE dbo.CarritoItems (
    CarritoItemId  INT IDENTITY(1,1) CONSTRAINT PK_CarritoItems PRIMARY KEY,
    UsuarioId      INT NOT NULL,
    VarianteId     INT NOT NULL,
    Cantidad       INT NOT NULL CONSTRAINT DF_CarritoItems_Cant DEFAULT 1,
    FechaAgregado  DATETIME2 NOT NULL CONSTRAINT DF_CarritoItems_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT CK_CarritoItems_Cant CHECK (Cantidad > 0),
    CONSTRAINT UQ_CarritoItems_UsuarioVariante UNIQUE (UsuarioId, VarianteId),
    CONSTRAINT FK_CarritoItems_Usuarios  FOREIGN KEY (UsuarioId)  REFERENCES dbo.Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_CarritoItems_Variantes FOREIGN KEY (VarianteId) REFERENCES dbo.Variantes(VarianteId) ON DELETE CASCADE
);
GO

-- 11. Cupones de descuento
CREATE TABLE dbo.Cupones (
    CuponId       INT IDENTITY(1,1) CONSTRAINT PK_Cupones PRIMARY KEY,
    Codigo        NVARCHAR(30)  NOT NULL CONSTRAINT UQ_Cupones_Codigo UNIQUE,
    Descripcion   NVARCHAR(150) NULL,
    Tipo          NVARCHAR(12)  NOT NULL CONSTRAINT DF_Cupones_Tipo DEFAULT N'Porcentaje',
    Valor         DECIMAL(18,2) NOT NULL,
    MinimoCompra  DECIMAL(18,2) NOT NULL CONSTRAINT DF_Cupones_Minimo DEFAULT 0,
    FechaInicio   DATE NOT NULL,
    FechaFin      DATE NOT NULL,
    UsosMaximos   INT NULL,
    UsosActuales  INT NOT NULL CONSTRAINT DF_Cupones_Usos DEFAULT 0,
    Activo        BIT NOT NULL CONSTRAINT DF_Cupones_Activo DEFAULT 1,
    CONSTRAINT CK_Cupones_Tipo CHECK (Tipo IN (N'Porcentaje', N'Monto')),
    CONSTRAINT CK_Cupones_Valor CHECK (Valor > 0),
    CONSTRAINT CK_Cupones_Fechas CHECK (FechaFin >= FechaInicio)
);
GO

-- 12. Métodos de envío
CREATE TABLE dbo.MetodosEnvio (
    MetodoEnvioId INT IDENTITY(1,1) CONSTRAINT PK_MetodosEnvio PRIMARY KEY,
    Nombre        NVARCHAR(60)  NOT NULL CONSTRAINT UQ_MetodosEnvio_Nombre UNIQUE,
    Costo         DECIMAL(18,2) NOT NULL CONSTRAINT DF_MetodosEnvio_Costo DEFAULT 0,
    DiasEstimados INT NOT NULL CONSTRAINT DF_MetodosEnvio_Dias DEFAULT 3,
    Activo        BIT NOT NULL CONSTRAINT DF_MetodosEnvio_Activo DEFAULT 1,
    CONSTRAINT CK_MetodosEnvio_Costo CHECK (Costo >= 0)
);
GO

-- 13. Pedidos (compras). El Folio (PJ-000001) lo calcula la base de datos.
CREATE TABLE dbo.Pedidos (
    PedidoId           INT IDENTITY(1,1) CONSTRAINT PK_Pedidos PRIMARY KEY,
    Folio              AS (N'PJ-' + RIGHT(N'000000' + CAST(PedidoId AS NVARCHAR(10)), 6)) PERSISTED,
    UsuarioId          INT NOT NULL,
    FechaHora          DATETIME2 NOT NULL CONSTRAINT DF_Pedidos_Fecha DEFAULT SYSDATETIME(),
    Subtotal           DECIMAL(18,2) NOT NULL,
    Descuento          DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Descuento DEFAULT 0,
    CostosEnvio        DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Envio DEFAULT 0,
    Total              DECIMAL(18,2) NOT NULL,
    EstadoPedido       NVARCHAR(20)  NOT NULL CONSTRAINT DF_Pedidos_Estado DEFAULT N'Pagado',
    MetodoPago         NVARCHAR(40)  NOT NULL,
    MetodoEnvioId      INT NULL,
    CuponId            INT NULL,
    NombreDestinatario NVARCHAR(100) NOT NULL,
    DireccionEnvio     NVARCHAR(250) NOT NULL,
    Ciudad             NVARCHAR(100) NOT NULL,
    CodigoPostal       NVARCHAR(10)  NOT NULL,
    Telefono           NVARCHAR(20)  NULL,
    CONSTRAINT CK_Pedidos_Estado CHECK (EstadoPedido IN (N'Pagado', N'Preparando', N'Enviado', N'Entregado', N'Cancelado')),
    CONSTRAINT CK_Pedidos_Total CHECK (Total >= 0),
    CONSTRAINT FK_Pedidos_Usuarios     FOREIGN KEY (UsuarioId)     REFERENCES dbo.Usuarios(UsuarioId),
    CONSTRAINT FK_Pedidos_MetodosEnvio FOREIGN KEY (MetodoEnvioId) REFERENCES dbo.MetodosEnvio(MetodoEnvioId),
    CONSTRAINT FK_Pedidos_Cupones      FOREIGN KEY (CuponId)       REFERENCES dbo.Cupones(CuponId)
);
GO

-- 14. Detalle de cada pedido (guarda nombre/talla/color/precio del momento de la compra)
CREATE TABLE dbo.DetallePedidos (
    DetallePedidoId INT IDENTITY(1,1) CONSTRAINT PK_DetallePedidos PRIMARY KEY,
    PedidoId        INT NOT NULL,
    ProductoId      INT NOT NULL,
    VarianteId      INT NOT NULL,
    NombreProducto  NVARCHAR(100) NOT NULL,
    Talla           NVARCHAR(10)  NOT NULL,
    Color           NVARCHAR(40)  NOT NULL,
    Cantidad        INT NOT NULL,
    PrecioUnitario  DECIMAL(18,2) NOT NULL,
    Importe         AS (Cantidad * PrecioUnitario) PERSISTED,
    CONSTRAINT CK_DetallePedidos_Cant CHECK (Cantidad > 0),
    CONSTRAINT FK_DetallePedidos_Pedidos   FOREIGN KEY (PedidoId)    REFERENCES dbo.Pedidos(PedidoId) ON DELETE CASCADE,
    CONSTRAINT FK_DetallePedidos_Productos FOREIGN KEY (ProductoId)  REFERENCES dbo.Productos(ProductoId),
    CONSTRAINT FK_DetallePedidos_Variantes FOREIGN KEY (VarianteId)  REFERENCES dbo.Variantes(VarianteId)
);
GO

-- 15. Pagos de cada pedido (simulados)
CREATE TABLE dbo.Pagos (
    PagoId     INT IDENTITY(1,1) CONSTRAINT PK_Pagos PRIMARY KEY,
    PedidoId   INT NOT NULL,
    MetodoPago NVARCHAR(40)  NOT NULL,
    Monto      DECIMAL(18,2) NOT NULL,
    EstadoPago NVARCHAR(20)  NOT NULL CONSTRAINT DF_Pagos_Estado DEFAULT N'Aprobado',
    Referencia NVARCHAR(50)  NULL,
    FechaPago  DATETIME2 NOT NULL CONSTRAINT DF_Pagos_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Pagos_Estado CHECK (EstadoPago IN (N'Aprobado', N'Reembolsado')),
    CONSTRAINT FK_Pagos_Pedidos FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos(PedidoId) ON DELETE CASCADE
);
GO

-- 16. Historial de estados del pedido (seguimiento)
CREATE TABLE dbo.HistorialEstados (
    HistorialEstadoId INT IDENTITY(1,1) CONSTRAINT PK_HistorialEstados PRIMARY KEY,
    PedidoId          INT NOT NULL,
    Estado            NVARCHAR(20)  NOT NULL,
    Comentario        NVARCHAR(250) NULL,
    RealizadoPor      NVARCHAR(100) NULL,
    Fecha             DATETIME2 NOT NULL CONSTRAINT DF_HistorialEstados_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT FK_HistorialEstados_Pedidos FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos(PedidoId) ON DELETE CASCADE
);
GO

-- 17. Reseñas de productos (una por cliente y producto)
CREATE TABLE dbo.Resenas (
    ResenaId     INT IDENTITY(1,1) CONSTRAINT PK_Resenas PRIMARY KEY,
    UsuarioId    INT NOT NULL,
    ProductoId   INT NOT NULL,
    Calificacion INT NOT NULL,
    Comentario   NVARCHAR(500) NULL,
    Fecha        DATETIME2 NOT NULL CONSTRAINT DF_Resenas_Fecha DEFAULT SYSDATETIME(),
    Visible      BIT NOT NULL CONSTRAINT DF_Resenas_Visible DEFAULT 1,
    CONSTRAINT CK_Resenas_Calif CHECK (Calificacion BETWEEN 1 AND 5),
    CONSTRAINT UQ_Resenas_UsuarioProducto UNIQUE (UsuarioId, ProductoId),
    CONSTRAINT FK_Resenas_Usuarios   FOREIGN KEY (UsuarioId)  REFERENCES dbo.Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_Resenas_Productos  FOREIGN KEY (ProductoId) REFERENCES dbo.Productos(ProductoId) ON DELETE CASCADE
);
GO

-- 18. Solicitudes de cotización (formulario "Contáctanos")
CREATE TABLE dbo.SolicitudesContacto (
    SolicitudId       INT IDENTITY(1,1) CONSTRAINT PK_SolicitudesContacto PRIMARY KEY,
    NombreCompleto    NVARCHAR(100) NOT NULL,
    Email             NVARCHAR(100) NOT NULL,
    PlayeraSolicitada NVARCHAR(100) NOT NULL,
    Talla             NVARCHAR(10)  NOT NULL,
    Detalles          NVARCHAR(500) NULL,
    FechaSolicitud    DATETIME2 NOT NULL CONSTRAINT DF_SolicitudesContacto_Fecha DEFAULT SYSDATETIME(),
    Atendida          BIT NOT NULL CONSTRAINT DF_SolicitudesContacto_Atendida DEFAULT 0
);
GO

/* ---------- Índices para consultas frecuentes ---------- */
CREATE INDEX IX_Productos_Categoria   ON dbo.Productos(CategoriaId);
CREATE INDEX IX_Productos_Coleccion   ON dbo.Productos(ColeccionId);
CREATE INDEX IX_Pedidos_Usuario       ON dbo.Pedidos(UsuarioId);
CREATE INDEX IX_Pedidos_Fecha         ON dbo.Pedidos(FechaHora);
CREATE INDEX IX_DetallePedidos_Pedido ON dbo.DetallePedidos(PedidoId);
CREATE INDEX IX_DetallePedidos_Prod   ON dbo.DetallePedidos(ProductoId);
CREATE INDEX IX_Pagos_Pedido          ON dbo.Pagos(PedidoId);
CREATE INDEX IX_Historial_Pedido      ON dbo.HistorialEstados(PedidoId);
CREATE INDEX IX_Resenas_Producto      ON dbo.Resenas(ProductoId);
GO

/* ============================== DATOS DE EJEMPLO ============================== */

SET IDENTITY_INSERT dbo.Usuarios ON;
INSERT INTO dbo.Usuarios (UsuarioId, Nombre, Email, PasswordHash, Telefono, Rol, Activo, FechaRegistro) VALUES
    (1, N'Administrador JyC', N'admin@playerasjyc.com', N'7v+POGrzBimn1RSDJ2n7zw==.dtJSkMDixQ9VfOVzjRCs41c6szM26HLZcyc3s6y87bw=', N'9611000001', N'Admin', 1, N'2026-01-05T09:00:00'),
    (2, N'Cliente Demo', N'cliente@playerasjyc.com', N'YyivWhrhV6GJ1DNRqdTrJA==.EO7jIge48emGGJNql5x4tIC2XbCLeA6gW5J8UmNJUpY=', N'9612000001', N'Cliente', 1, N'2026-06-10T10:00:00'),
    (3, N'María Pérez', N'maria@example.com', N'YyivWhrhV6GJ1DNRqdTrJA==.EO7jIge48emGGJNql5x4tIC2XbCLeA6gW5J8UmNJUpY=', N'9612000002', N'Cliente', 1, N'2026-07-02T11:30:00'),
    (4, N'Juan Hernández', N'juan@example.com', N'YyivWhrhV6GJ1DNRqdTrJA==.EO7jIge48emGGJNql5x4tIC2XbCLeA6gW5J8UmNJUpY=', N'9612000003', N'Cliente', 1, N'2026-07-20T15:45:00');
SET IDENTITY_INSERT dbo.Usuarios OFF;
GO

SET IDENTITY_INSERT dbo.Direcciones ON;
INSERT INTO dbo.Direcciones (DireccionId, UsuarioId, Alias, NombreDestinatario, Calle, Colonia, Ciudad, Estado, CodigoPostal, Telefono, Predeterminada) VALUES
    (1, 2, N'Casa', N'Cliente Demo', N'Av. Central 120', N'Centro', N'Tuxtla Gutiérrez', N'Chiapas', N'29000', N'9612000001', 1),
    (2, 3, N'Casa', N'María Pérez', N'Calle Hidalgo 45', N'San Roque', N'San Cristóbal de las Casas', N'Chiapas', N'29220', N'9612000002', 1),
    (3, 4, N'Trabajo', N'Juan Hernández', N'Blvd. Belisario Domínguez 800', N'Moctezuma', N'Tuxtla Gutiérrez', N'Chiapas', N'29030', N'9612000003', 1);
SET IDENTITY_INSERT dbo.Direcciones OFF;
GO

SET IDENTITY_INSERT dbo.Categorias ON;
INSERT INTO dbo.Categorias (CategoriaId, Nombre, Descripcion, Activo) VALUES
    (1, N'Oversize', N'Corte holgado y cómodo, estilo urbano.', 1),
    (2, N'Básicas', N'Playeras lisas de uso diario.', 1),
    (3, N'Estampadas', N'Diseños exclusivos de la casa.', 1),
    (4, N'Deportivas', N'Tela técnica para entrenar.', 1),
    (5, N'Personalizadas', N'Hechas con tu propio diseño.', 1);
SET IDENTITY_INSERT dbo.Categorias OFF;
GO

SET IDENTITY_INSERT dbo.Colecciones ON;
INSERT INTO dbo.Colecciones (ColeccionId, Nombre, Descripcion, Activo) VALUES
    (1, N'Neon Nights', N'Diseños que brillan en la noche.', 1),
    (2, N'Street Classics', N'Los clásicos del streetwear.', 1),
    (3, N'Temporada 2026', N'Lo nuevo de este año.', 1);
SET IDENTITY_INSERT dbo.Colecciones OFF;
GO

SET IDENTITY_INSERT dbo.Tallas ON;
INSERT INTO dbo.Tallas (TallaId, Nombre, Orden) VALUES
    (1, N'S', 1),
    (2, N'M', 2),
    (3, N'L', 3),
    (4, N'XL', 4),
    (5, N'XXL', 5);
SET IDENTITY_INSERT dbo.Tallas OFF;
GO

SET IDENTITY_INSERT dbo.Colores ON;
INSERT INTO dbo.Colores (ColorId, Nombre, CodigoHex) VALUES
    (1, N'Negro', N'#111111'),
    (2, N'Blanco', N'#F5F5F5'),
    (3, N'Rosa Neón', N'#FF007F'),
    (4, N'Cian', N'#00F0FF'),
    (5, N'Gris', N'#8A8AA5'),
    (6, N'Morado', N'#7B2FF7');
SET IDENTITY_INSERT dbo.Colores OFF;
GO

SET IDENTITY_INSERT dbo.Productos ON;
INSERT INTO dbo.Productos (ProductoId, CategoriaId, ColeccionId, Nombre, Descripcion, Material, PrecioOriginal, EnOferta, PrecioOferta, ImagenUrl, Activo, FechaAlta) VALUES
    (1, 1, 1, N'Oversize Neon Rush', N'Corte oversize con estampado neón al frente. Caída holgada y tela suave.', N'100% algodón peinado 220 g', 449.00, 1, 349.00, N'/images/productos/p1.svg', 1, N'2026-06-01T10:00:00'),
    (2, 1, 2, N'Oversize Black Label', N'Oversize negra minimalista con etiqueta bordada en la espalda.', N'100% algodón 240 g', 429.00, 0, NULL, N'/images/productos/p2.svg', 1, N'2026-06-10T10:00:00'),
    (3, 2, 2, N'Básica Premium Blanca', N'La básica que no falla. Cuello reforzado y costuras dobles.', N'100% algodón pima', 249.00, 0, NULL, N'/images/productos/p3.svg', 1, N'2026-06-19T10:00:00'),
    (4, 2, 2, N'Básica Premium Negra', N'Negra profunda que no destiñe. Ideal para combinar con todo.', N'100% algodón pima', 249.00, 0, NULL, N'/images/productos/p4.svg', 1, N'2026-06-28T10:00:00'),
    (5, 3, 1, N'Cyber Cat', N'Gato cyberpunk con tinta que brilla bajo luz negra.', N'Algodón 180 g, serigrafía', 399.00, 1, 329.00, N'/images/productos/p5.svg', 1, N'2026-07-07T10:00:00'),
    (6, 3, 1, N'Skull Neon', N'Calavera neón en alta definición. Edición limitada.', N'Algodón 180 g, serigrafía', 419.00, 0, NULL, N'/images/productos/p6.svg', 1, N'2026-07-16T10:00:00'),
    (7, 4, 3, N'Dry-Fit Runner', N'Tela técnica que respira y seca rápido. Ideal para correr y gym.', N'Poliéster dry-fit', 379.00, 0, NULL, N'/images/productos/p7.svg', 1, N'2026-07-25T10:00:00'),
    (8, 4, 3, N'Pro Training', N'Entrena sin límites: costuras planas y tela elástica.', N'Poliéster y elastano', 399.00, 1, 319.00, N'/images/productos/p8.svg', 1, N'2026-08-03T10:00:00'),
    (9, 5, 2, N'Playera Personalizada', N'Tú eliges el diseño: cuéntanos tu idea en Contáctanos y la hacemos realidad.', N'Algodón 180 g', 459.00, 0, NULL, N'/images/productos/p9.svg', 1, N'2026-08-12T10:00:00'),
    (10, 3, 3, N'Retro Wave 85', N'Atardecer retro-wave con palmeras y rejilla. Pura nostalgia de los 80.', N'Algodón 180 g, serigrafía', 389.00, 1, 299.00, N'/images/productos/p10.svg', 1, N'2026-08-21T10:00:00'),
    (11, 1, 3, N'Oversize Pastel Dreams', N'Oversize en tonos suaves para un look relajado todos los días.', N'100% algodón 220 g', 439.00, 0, NULL, N'/images/productos/p11.svg', 1, N'2026-08-30T10:00:00'),
    (12, 3, 2, N'Street Graffiti Tag', N'Tag de graffiti urbano estampado a mano. Cada lote es único.', N'Algodón 200 g', 409.00, 0, NULL, N'/images/productos/p12.svg', 1, N'2026-09-08T10:00:00');
SET IDENTITY_INSERT dbo.Productos OFF;
GO

SET IDENTITY_INSERT dbo.ImagenesProducto ON;
INSERT INTO dbo.ImagenesProducto (ImagenProductoId, ProductoId, Url, Orden) VALUES
    (1, 1, N'/images/productos/p1.svg', 1),
    (2, 1, N'/images/productos/p1-b.svg', 2),
    (3, 2, N'/images/productos/p2.svg', 1),
    (4, 2, N'/images/productos/p2-b.svg', 2),
    (5, 3, N'/images/productos/p3.svg', 1),
    (6, 3, N'/images/productos/p3-b.svg', 2),
    (7, 4, N'/images/productos/p4.svg', 1),
    (8, 4, N'/images/productos/p4-b.svg', 2),
    (9, 5, N'/images/productos/p5.svg', 1),
    (10, 5, N'/images/productos/p5-b.svg', 2),
    (11, 6, N'/images/productos/p6.svg', 1),
    (12, 6, N'/images/productos/p6-b.svg', 2),
    (13, 7, N'/images/productos/p7.svg', 1),
    (14, 7, N'/images/productos/p7-b.svg', 2),
    (15, 8, N'/images/productos/p8.svg', 1),
    (16, 8, N'/images/productos/p8-b.svg', 2),
    (17, 9, N'/images/productos/p9.svg', 1),
    (18, 9, N'/images/productos/p9-b.svg', 2),
    (19, 10, N'/images/productos/p10.svg', 1),
    (20, 10, N'/images/productos/p10-b.svg', 2),
    (21, 11, N'/images/productos/p11.svg', 1),
    (22, 11, N'/images/productos/p11-b.svg', 2),
    (23, 12, N'/images/productos/p12.svg', 1),
    (24, 12, N'/images/productos/p12-b.svg', 2);
SET IDENTITY_INSERT dbo.ImagenesProducto OFF;
GO

SET IDENTITY_INSERT dbo.Variantes ON;
INSERT INTO dbo.Variantes (VarianteId, ProductoId, TallaId, ColorId, Sku, Stock) VALUES
    (1, 1, 1, 1, N'PJ-01-S-1', 15),
    (2, 1, 2, 1, N'PJ-01-M-1', 3),
    (3, 1, 3, 1, N'PJ-01-L-1', 7),
    (4, 1, 4, 1, N'PJ-01-XL-1', 10),
    (5, 1, 5, 1, N'PJ-01-XXL-1', 13),
    (6, 1, 1, 3, N'PJ-01-S-3', 5),
    (7, 1, 2, 3, N'PJ-01-M-3', 8),
    (8, 1, 3, 3, N'PJ-01-L-3', 11),
    (9, 1, 4, 3, N'PJ-01-XL-3', 14),
    (10, 1, 5, 3, N'PJ-01-XXL-3', 3),
    (11, 2, 1, 1, N'PJ-02-S-1', 8),
    (12, 2, 2, 1, N'PJ-02-M-1', 11),
    (13, 2, 3, 1, N'PJ-02-L-1', 14),
    (14, 2, 4, 1, N'PJ-02-XL-1', 2),
    (15, 2, 5, 1, N'PJ-02-XXL-1', 6),
    (16, 2, 1, 5, N'PJ-02-S-5', 16),
    (17, 2, 2, 5, N'PJ-02-M-5', 5),
    (18, 2, 3, 5, N'PJ-02-L-5', 8),
    (19, 2, 4, 5, N'PJ-02-XL-5', 0),
    (20, 2, 5, 5, N'PJ-02-XXL-5', 14),
    (21, 3, 1, 2, N'PJ-03-S-2', 3),
    (22, 3, 2, 2, N'PJ-03-M-2', 6),
    (23, 3, 3, 2, N'PJ-03-L-2', 7),
    (24, 3, 4, 2, N'PJ-03-XL-2', 12),
    (25, 3, 1, 5, N'PJ-03-S-5', 9),
    (26, 3, 2, 5, N'PJ-03-M-5', 12),
    (27, 3, 3, 5, N'PJ-03-L-5', 0),
    (28, 3, 4, 5, N'PJ-03-XL-5', 4),
    (29, 4, 1, 1, N'PJ-04-S-1', 8),
    (30, 4, 2, 1, N'PJ-04-M-1', 8),
    (31, 4, 3, 1, N'PJ-04-L-1', 14),
    (32, 4, 4, 1, N'PJ-04-XL-1', 3),
    (33, 4, 1, 6, N'PJ-04-S-6', 0),
    (34, 4, 2, 6, N'PJ-04-M-6', 7),
    (35, 4, 3, 6, N'PJ-04-L-6', 10),
    (36, 4, 4, 6, N'PJ-04-XL-6', 13),
    (37, 5, 1, 1, N'PJ-05-S-1', 15),
    (38, 5, 2, 1, N'PJ-05-M-1', 3),
    (39, 5, 3, 1, N'PJ-05-L-1', 7),
    (40, 5, 4, 1, N'PJ-05-XL-1', 10),
    (41, 5, 1, 4, N'PJ-05-S-4', 6),
    (42, 5, 2, 4, N'PJ-05-M-4', 0),
    (43, 5, 3, 4, N'PJ-05-L-4', 13),
    (44, 5, 4, 4, N'PJ-05-XL-4', 16),
    (45, 6, 1, 1, N'PJ-06-S-1', 8),
    (46, 6, 2, 1, N'PJ-06-M-1', 10),
    (47, 6, 3, 1, N'PJ-06-L-1', 14),
    (48, 6, 4, 1, N'PJ-06-XL-1', 0),
    (49, 6, 1, 3, N'PJ-06-S-3', 12),
    (50, 6, 2, 3, N'PJ-06-M-3', 0),
    (51, 6, 3, 3, N'PJ-06-L-3', 3),
    (52, 6, 4, 3, N'PJ-06-XL-3', 7),
    (53, 7, 1, 4, N'PJ-07-S-4', 7),
    (54, 7, 2, 4, N'PJ-07-M-4', 10),
    (55, 7, 3, 4, N'PJ-07-L-4', 11),
    (56, 7, 4, 4, N'PJ-07-XL-4', 16),
    (57, 7, 1, 1, N'PJ-07-S-1', 15),
    (58, 7, 2, 1, N'PJ-07-M-1', 4),
    (59, 7, 3, 1, N'PJ-07-L-1', 0),
    (60, 7, 4, 1, N'PJ-07-XL-1', 10),
    (61, 8, 1, 1, N'PJ-08-S-1', 8),
    (62, 8, 2, 1, N'PJ-08-M-1', 0),
    (63, 8, 3, 1, N'PJ-08-L-1', 14),
    (64, 8, 4, 1, N'PJ-08-XL-1', 3),
    (65, 8, 1, 6, N'PJ-08-S-6', 4),
    (66, 8, 2, 6, N'PJ-08-M-6', 7),
    (67, 8, 3, 6, N'PJ-08-L-6', 10),
    (68, 8, 4, 6, N'PJ-08-XL-6', 13),
    (69, 9, 1, 2, N'PJ-09-S-2', 3),
    (70, 9, 2, 2, N'PJ-09-M-2', 6),
    (71, 9, 3, 2, N'PJ-09-L-2', 8),
    (72, 9, 4, 2, N'PJ-09-XL-2', 12),
    (73, 9, 1, 1, N'PJ-09-S-1', 0),
    (74, 9, 2, 1, N'PJ-09-M-1', 4),
    (75, 9, 3, 1, N'PJ-09-L-1', 7),
    (76, 9, 4, 1, N'PJ-09-XL-1', 10),
    (77, 10, 1, 6, N'PJ-10-S-6', 4),
    (78, 10, 2, 6, N'PJ-10-M-6', 6),
    (79, 10, 3, 6, N'PJ-10-L-6', 10),
    (80, 10, 4, 6, N'PJ-10-XL-6', 13),
    (81, 10, 1, 1, N'PJ-10-S-1', 8),
    (82, 10, 2, 1, N'PJ-10-M-1', 11),
    (83, 10, 3, 1, N'PJ-10-L-1', 14),
    (84, 10, 4, 1, N'PJ-10-XL-1', 3),
    (85, 11, 1, 2, N'PJ-11-S-2', 2),
    (86, 11, 2, 2, N'PJ-11-M-2', 6),
    (87, 11, 3, 2, N'PJ-11-L-2', 9),
    (88, 11, 4, 2, N'PJ-11-XL-2', 12),
    (89, 11, 5, 2, N'PJ-11-XXL-2', 15),
    (90, 11, 1, 3, N'PJ-11-S-3', 5),
    (91, 11, 2, 3, N'PJ-11-M-3', 8),
    (92, 11, 3, 3, N'PJ-11-L-3', 11),
    (93, 11, 4, 3, N'PJ-11-XL-3', 14),
    (94, 11, 5, 3, N'PJ-11-XXL-3', 3),
    (95, 12, 1, 5, N'PJ-12-S-5', 16),
    (96, 12, 2, 5, N'PJ-12-M-5', 4),
    (97, 12, 3, 5, N'PJ-12-L-5', 8),
    (98, 12, 4, 5, N'PJ-12-XL-5', 11),
    (99, 12, 1, 1, N'PJ-12-S-1', 8),
    (100, 12, 2, 1, N'PJ-12-M-1', 11),
    (101, 12, 3, 1, N'PJ-12-L-1', 14),
    (102, 12, 4, 1, N'PJ-12-XL-1', 3);
SET IDENTITY_INSERT dbo.Variantes OFF;
GO

SET IDENTITY_INSERT dbo.Cupones ON;
INSERT INTO dbo.Cupones (CuponId, Codigo, Descripcion, Tipo, Valor, MinimoCompra, FechaInicio, FechaFin, UsosMaximos, UsosActuales, Activo) VALUES
    (1, N'BIENVENIDO10', N'10% de descuento en tu compra', N'Porcentaje', 10.00, 0.00, N'2026-01-01', N'2026-12-31', NULL, 4, 1),
    (2, N'NEON50', N'$50 de descuento en compras desde $400', N'Monto', 50.00, 400.00, N'2026-01-01', N'2026-12-31', 200, 1, 1),
    (3, N'PLAYERAS15', N'15% de descuento en compras desde $800', N'Porcentaje', 15.00, 800.00, N'2026-01-01', N'2026-12-31', 100, 0, 1);
SET IDENTITY_INSERT dbo.Cupones OFF;
GO

SET IDENTITY_INSERT dbo.MetodosEnvio ON;
INSERT INTO dbo.MetodosEnvio (MetodoEnvioId, Nombre, Costo, DiasEstimados, Activo) VALUES
    (1, N'Estándar', 80.00, 5, 1),
    (2, N'Express', 150.00, 2, 1),
    (3, N'Recoger en tienda', 0.00, 1, 1);
SET IDENTITY_INSERT dbo.MetodosEnvio OFF;
GO

SET IDENTITY_INSERT dbo.Pedidos ON;
INSERT INTO dbo.Pedidos (PedidoId, UsuarioId, FechaHora, Subtotal, Descuento, CostosEnvio, Total, EstadoPedido, MetodoPago, MetodoEnvioId, CuponId, NombreDestinatario, DireccionEnvio, Ciudad, CodigoPostal, Telefono) VALUES
    (1, 2, N'2026-08-12T10:30:00', 847.00, 0.00, 80.00, 927.00, N'Entregado', N'Tarjeta', 1, NULL, N'Cliente Demo', N'Av. Central 120, Centro, Chiapas', N'Tuxtla Gutiérrez', N'29000', N'9612000001'),
    (2, 3, N'2026-08-28T16:05:00', 329.00, 32.90, 150.00, 446.10, N'Entregado', N'Transferencia', 2, 1, N'María Pérez', N'Calle Hidalgo 45, San Roque, Chiapas', N'San Cristóbal de las Casas', N'29220', N'9612000002'),
    (3, 4, N'2026-09-05T12:40:00', 848.00, 0.00, 80.00, 928.00, N'Entregado', N'Tarjeta', 1, NULL, N'Juan Hernández', N'Blvd. Belisario Domínguez 800, Moctezuma, Chiapas', N'Tuxtla Gutiérrez', N'29030', N'9612000003'),
    (4, 2, N'2026-09-19T19:15:00', 638.00, 50.00, 0.00, 588.00, N'Entregado', N'Depósito OXXO', 3, 2, N'Cliente Demo', N'Av. Central 120, Centro, Chiapas', N'Tuxtla Gutiérrez', N'29000', N'9612000001'),
    (5, 3, N'2026-10-02T11:20:00', 758.00, 75.80, 80.00, 762.20, N'Entregado', N'Tarjeta', 1, 1, N'María Pérez', N'Calle Hidalgo 45, San Roque, Chiapas', N'San Cristóbal de las Casas', N'29220', N'9612000002'),
    (6, 4, N'2026-10-15T09:50:00', 758.00, 0.00, 80.00, 838.00, N'Enviado', N'Tarjeta', 1, NULL, N'Juan Hernández', N'Blvd. Belisario Domínguez 800, Moctezuma, Chiapas', N'Tuxtla Gutiérrez', N'29030', N'9612000003'),
    (7, 2, N'2026-10-28T14:10:00', 848.00, 84.80, 150.00, 913.20, N'Preparando', N'Tarjeta', 2, 1, N'Cliente Demo', N'Av. Central 120, Centro, Chiapas', N'Tuxtla Gutiérrez', N'29000', N'9612000001'),
    (8, 3, N'2026-11-03T18:45:00', 747.00, 0.00, 80.00, 827.00, N'Pagado', N'Transferencia', 1, NULL, N'María Pérez', N'Calle Hidalgo 45, San Roque, Chiapas', N'San Cristóbal de las Casas', N'29220', N'9612000002'),
    (9, 4, N'2026-11-06T13:00:00', 349.00, 0.00, 0.00, 349.00, N'Cancelado', N'Tarjeta', 3, NULL, N'Juan Hernández', N'Blvd. Belisario Domínguez 800, Moctezuma, Chiapas', N'Tuxtla Gutiérrez', N'29030', N'9612000003'),
    (10, 2, N'2026-11-08T20:30:00', 748.00, 74.80, 80.00, 753.20, N'Pagado', N'Tarjeta', 1, 1, N'Cliente Demo', N'Av. Central 120, Centro, Chiapas', N'Tuxtla Gutiérrez', N'29000', N'9612000001');
SET IDENTITY_INSERT dbo.Pedidos OFF;
GO

SET IDENTITY_INSERT dbo.DetallePedidos ON;
INSERT INTO dbo.DetallePedidos (DetallePedidoId, PedidoId, ProductoId, VarianteId, NombreProducto, Talla, Color, Cantidad, PrecioUnitario) VALUES
    (1, 1, 1, 2, N'Oversize Neon Rush', N'M', N'Negro', 1, 349.00),
    (2, 1, 3, 23, N'Básica Premium Blanca', N'L', N'Blanco', 2, 249.00),
    (3, 2, 5, 41, N'Cyber Cat', N'S', N'Cian', 1, 329.00),
    (4, 3, 2, 14, N'Oversize Black Label', N'XL', N'Negro', 1, 429.00),
    (5, 3, 6, 51, N'Skull Neon', N'L', N'Rosa Neón', 1, 419.00),
    (6, 4, 8, 62, N'Pro Training', N'M', N'Negro', 2, 319.00),
    (7, 5, 10, 78, N'Retro Wave 85', N'M', N'Morado', 1, 299.00),
    (8, 5, 9, 71, N'Playera Personalizada', N'L', N'Blanco', 1, 459.00),
    (9, 6, 7, 55, N'Dry-Fit Runner', N'L', N'Cian', 2, 379.00),
    (10, 7, 11, 85, N'Oversize Pastel Dreams', N'S', N'Blanco', 1, 439.00),
    (11, 7, 12, 96, N'Street Graffiti Tag', N'M', N'Gris', 1, 409.00),
    (12, 8, 4, 30, N'Básica Premium Negra', N'M', N'Negro', 3, 249.00),
    (13, 9, 1, 8, N'Oversize Neon Rush', N'L', N'Rosa Neón', 1, 349.00),
    (14, 10, 5, 38, N'Cyber Cat', N'M', N'Negro', 1, 329.00),
    (15, 10, 6, 46, N'Skull Neon', N'M', N'Negro', 1, 419.00);
SET IDENTITY_INSERT dbo.DetallePedidos OFF;
GO

SET IDENTITY_INSERT dbo.Pagos ON;
INSERT INTO dbo.Pagos (PagoId, PedidoId, MetodoPago, Monto, EstadoPago, Referencia, FechaPago) VALUES
    (1, 1, N'Tarjeta', 927.00, N'Aprobado', N'PJ260812103000101', N'2026-08-12T10:30:00'),
    (2, 2, N'Transferencia', 446.10, N'Aprobado', N'PJ260828160500102', N'2026-08-28T16:05:00'),
    (3, 3, N'Tarjeta', 928.00, N'Aprobado', N'PJ260905124000103', N'2026-09-05T12:40:00'),
    (4, 4, N'Depósito OXXO', 588.00, N'Aprobado', N'PJ260919191500104', N'2026-09-19T19:15:00'),
    (5, 5, N'Tarjeta', 762.20, N'Aprobado', N'PJ261002112000105', N'2026-10-02T11:20:00'),
    (6, 6, N'Tarjeta', 838.00, N'Aprobado', N'PJ261015095000106', N'2026-10-15T09:50:00'),
    (7, 7, N'Tarjeta', 913.20, N'Aprobado', N'PJ261028141000107', N'2026-10-28T14:10:00'),
    (8, 8, N'Transferencia', 827.00, N'Aprobado', N'PJ261103184500108', N'2026-11-03T18:45:00'),
    (9, 9, N'Tarjeta', 349.00, N'Reembolsado', N'PJ261106130000109', N'2026-11-06T13:00:00'),
    (10, 10, N'Tarjeta', 753.20, N'Aprobado', N'PJ261108203000110', N'2026-11-08T20:30:00');
SET IDENTITY_INSERT dbo.Pagos OFF;
GO

SET IDENTITY_INSERT dbo.HistorialEstados ON;
INSERT INTO dbo.HistorialEstados (HistorialEstadoId, PedidoId, Estado, Comentario, RealizadoPor, Fecha) VALUES
    (1, 1, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-08-12T10:30:00'),
    (2, 1, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-08-13T10:30:00'),
    (3, 1, N'Enviado', N'Tu pedido va en camino.', N'Administrador JyC', N'2026-08-14T10:30:00'),
    (4, 1, N'Entregado', N'Pedido entregado.', N'Administrador JyC', N'2026-08-17T10:30:00'),
    (5, 2, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-08-28T16:05:00'),
    (6, 2, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-08-29T16:05:00'),
    (7, 2, N'Enviado', N'Tu pedido va en camino.', N'Administrador JyC', N'2026-08-30T16:05:00'),
    (8, 2, N'Entregado', N'Pedido entregado.', N'Administrador JyC', N'2026-09-02T16:05:00'),
    (9, 3, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-09-05T12:40:00'),
    (10, 3, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-09-06T12:40:00'),
    (11, 3, N'Enviado', N'Tu pedido va en camino.', N'Administrador JyC', N'2026-09-07T12:40:00'),
    (12, 3, N'Entregado', N'Pedido entregado.', N'Administrador JyC', N'2026-09-10T12:40:00'),
    (13, 4, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-09-19T19:15:00'),
    (14, 4, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-09-20T19:15:00'),
    (15, 4, N'Enviado', N'Tu pedido va en camino.', N'Administrador JyC', N'2026-09-21T19:15:00'),
    (16, 4, N'Entregado', N'Pedido entregado.', N'Administrador JyC', N'2026-09-24T19:15:00'),
    (17, 5, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-10-02T11:20:00'),
    (18, 5, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-10-03T11:20:00'),
    (19, 5, N'Enviado', N'Tu pedido va en camino.', N'Administrador JyC', N'2026-10-04T11:20:00'),
    (20, 5, N'Entregado', N'Pedido entregado.', N'Administrador JyC', N'2026-10-07T11:20:00'),
    (21, 6, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-10-15T09:50:00'),
    (22, 6, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-10-16T09:50:00'),
    (23, 6, N'Enviado', N'Tu pedido va en camino.', N'Administrador JyC', N'2026-10-17T09:50:00'),
    (24, 7, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-10-28T14:10:00'),
    (25, 7, N'Preparando', N'Estamos preparando tu pedido.', N'Administrador JyC', N'2026-10-29T14:10:00'),
    (26, 8, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-11-03T18:45:00'),
    (27, 9, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-11-06T13:00:00'),
    (28, 9, N'Cancelado', N'Cancelado por el cliente. Pago reembolsado.', N'Juan Hernández', N'2026-11-07T13:00:00'),
    (29, 10, N'Pagado', N'Pedido recibido y pago aprobado.', N'Sistema', N'2026-11-08T20:30:00');
SET IDENTITY_INSERT dbo.HistorialEstados OFF;
GO

SET IDENTITY_INSERT dbo.Resenas ON;
INSERT INTO dbo.Resenas (ResenaId, UsuarioId, ProductoId, Calificacion, Comentario, Fecha, Visible) VALUES
    (1, 2, 1, 5, N'Muy cómoda y la tela es excelente.', N'2026-10-06T12:00:00', 1),
    (2, 3, 5, 5, N'El estampado brilla increíble, me encantó.', N'2026-10-07T12:00:00', 1),
    (3, 4, 2, 4, N'Gran caída, la talla XL me quedó perfecta.', N'2026-10-08T12:00:00', 1),
    (4, 2, 8, 5, N'Perfecta para entrenar, no se siente pesada.', N'2026-10-09T12:00:00', 1),
    (5, 3, 10, 4, N'Diseño retro muy padre, llegó rápido.', N'2026-10-10T12:00:00', 1),
    (6, 4, 6, 5, N'La calavera se ve increíble en persona.', N'2026-10-11T12:00:00', 1),
    (7, 2, 3, 4, N'Básica de calidad, ya pedí otra.', N'2026-10-12T12:00:00', 1),
    (8, 3, 9, 5, N'Quedó justo como la imaginé.', N'2026-10-13T12:00:00', 1);
SET IDENTITY_INSERT dbo.Resenas OFF;
GO

SET IDENTITY_INSERT dbo.SolicitudesContacto ON;
INSERT INTO dbo.SolicitudesContacto (SolicitudId, NombreCompleto, Email, PlayeraSolicitada, Talla, Detalles, FechaSolicitud, Atendida) VALUES
    (1, N'Ana Torres', N'ana@example.com', N'Playera con logo de mi equipo', N'M', N'Quiero 12 piezas con el escudo al frente, color azul.', N'2026-11-04T10:15:00', 0),
    (2, N'Carlos Ruiz', N'carlos@example.com', N'Oversize personalizada', N'XL', N'Diseño en la espalda, adjunto la imagen por correo.', N'2026-10-22T17:40:00', 1);
SET IDENTITY_INSERT dbo.SolicitudesContacto OFF;
GO

/* ---------- Comprobación rápida ---------- */
SELECT 'Usuarios' AS Tabla, COUNT(*) AS Registros FROM dbo.Usuarios
UNION ALL SELECT 'Productos', COUNT(*) FROM dbo.Productos
UNION ALL SELECT 'Variantes', COUNT(*) FROM dbo.Variantes
UNION ALL SELECT 'Pedidos', COUNT(*) FROM dbo.Pedidos
UNION ALL SELECT 'DetallePedidos', COUNT(*) FROM dbo.DetallePedidos;
GO
