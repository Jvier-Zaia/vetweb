-- ============================================================================
-- TRABAJO PRÁCTICO INTEGRADOR - PROGRAMACIÓN 2
-- Temática: Registro de Mascotas en Veterinaria (Opción 7)
-- Base de Datos: veterinaria_db (MariaDB / MySQL en HeidiSQL)
-- ============================================================================

DROP DATABASE IF EXISTS veterinaria_db;
CREATE DATABASE veterinaria_db CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;
USE veterinaria_db;

-- ============================================================================
-- 1. TABLAS PRINCIPALES DEL TRABAJO PRÁCTICO (3 TABLAS RELACIONADAS)
-- ============================================================================

-- Tabla Auxiliar 1: Especies
CREATE TABLE especies (
    id_especie INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB;

-- Tabla Auxiliar 2: Dueños
CREATE TABLE duenios (
    id_duenio INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(60) NOT NULL,
    apellido VARCHAR(60) NOT NULL,
    telefono VARCHAR(25) NOT NULL,
    email VARCHAR(100) NOT NULL
) ENGINE=InnoDB;

-- Tabla Principal (Entidad Central): Mascotas
-- Vinculada con Claves Foráneas (FK) hacia 'especies' y 'duenios'
CREATE TABLE mascotas (
    id_mascota INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(60) NOT NULL,
    id_especie INT NOT NULL,
    id_duenio INT NOT NULL,
    raza VARCHAR(60),
    edad INT DEFAULT 0,
    peso DECIMAL(5,2),
    CONSTRAINT fk_mascota_especie FOREIGN KEY (id_especie) 
        REFERENCES especies(id_especie) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_mascota_duenio FOREIGN KEY (id_duenio) 
        REFERENCES duenios(id_duenio) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB;

-- ============================================================================
-- 2. TABLAS EXTENDIDAS (TIENDA, TURNOS Y PEDIDOS)
-- ============================================================================

CREATE TABLE categorias_productos (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(60) NOT NULL UNIQUE,
    descripcion VARCHAR(150)
) ENGINE=InnoDB;

CREATE TABLE proveedores (
    id_proveedor INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(80) NOT NULL,
    contacto VARCHAR(60),
    telefono VARCHAR(25)
) ENGINE=InnoDB;

CREATE TABLE productos (
    id_producto INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    id_categoria INT NOT NULL,
    id_proveedor INT NOT NULL,
    precio DECIMAL(10,2) NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    descripcion TEXT,
    CONSTRAINT fk_producto_categoria FOREIGN KEY (id_categoria) REFERENCES categorias_productos(id_categoria) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_producto_proveedor FOREIGN KEY (id_proveedor) REFERENCES proveedores(id_proveedor) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE veterinarios (
    id_veterinario INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(60) NOT NULL,
    apellido VARCHAR(60) NOT NULL,
    matricula VARCHAR(30) NOT NULL UNIQUE,
    especialidad VARCHAR(60) DEFAULT 'Clínica General'
) ENGINE=InnoDB;

CREATE TABLE turnos (
    id_turno INT AUTO_INCREMENT PRIMARY KEY,
    id_mascota INT NOT NULL,
    id_veterinario INT NOT NULL,
    fecha_hora DATETIME NOT NULL,
    motivo VARCHAR(200) NOT NULL,
    estado ENUM('Pendiente', 'En Consulta', 'Finalizado', 'Cancelado') DEFAULT 'Pendiente',
    CONSTRAINT fk_turno_mascota FOREIGN KEY (id_mascota) REFERENCES mascotas(id_mascota) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_turno_veterinario FOREIGN KEY (id_veterinario) REFERENCES veterinarios(id_veterinario) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE pedidos (
    id_pedido INT AUTO_INCREMENT PRIMARY KEY,
    cliente_nombre VARCHAR(80) NOT NULL,
    cliente_telefono VARCHAR(25) NOT NULL,
    cliente_email VARCHAR(100) NOT NULL,
    total DECIMAL(10,2) NOT NULL,
    alias_destino VARCHAR(50) DEFAULT 'vetweb.oficial',
    nro_operacion VARCHAR(60),
    comprobante_nombre VARCHAR(150),
    estado ENUM('Pendiente', 'Verificado', 'Rechazado') DEFAULT 'Verificado',
    fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    detalle_items TEXT
) ENGINE=InnoDB;

-- ============================================================================
-- 3. DATOS DE PRUEBA REALISTAS (INSERT)
-- ============================================================================

-- Especies iniciales
INSERT INTO especies (nombre) VALUES 
('Perro'), 
('Gato'), 
('Ave'), 
('Conejo'), 
('Lobo Marino');

-- Dueños iniciales
INSERT INTO duenios (nombre, apellido, telefono, email) VALUES 
('Carlos', 'Gómez', '11-4567-8901', 'carlos.gomez@mail.com'),
('Sofía', 'Herrera', '11-7890-1234', 'sofia.herrera@mail.com'),
('Martín', 'Palermo', '11-3456-7890', 'martin.palermo@mail.com'),
('Monty', 'Widenius', '11-9876-5432', 'monty@mariadb.org');

-- Mascotas vinculadas a Especie y Dueño
INSERT INTO mascotas (nombre, id_especie, id_duenio, raza, edad, peso) VALUES 
('Max', 1, 1, 'Golden Retriever', 3, 31.50),
('Luna', 2, 2, 'Siamés', 2, 4.20),
('Thor', 1, 3, 'Bulldog Francés', 4, 12.80),
('Marina', 5, 4, 'Lobo Fino (Mascota MariaDB)', 4, 85.00);

-- Datos de Tienda Pet Shop
INSERT INTO categorias_productos (nombre, descripcion) VALUES 
('Alimentos', 'Balanceados y golosinas para mascotas'),
('Farmacia', 'Antiparasitarios, vacunas y medicamentos'),
('Accesorios', 'Correas, juguetes, camas y platos');

INSERT INTO proveedores (nombre, contacto, telefono) VALUES 
('Royal Canin Argentina', 'Lucas Ventas', '11-4000-1111'),
('Laboratorios Zoetis', 'Dra. Romina', '11-4000-2222'),
('Pet Mayorista S.A.', 'Esteban', '11-4000-3333');

INSERT INTO productos (nombre, id_categoria, id_proveedor, precio, stock, descripcion) VALUES 
('Royal Canin Maxi Adult 15kg', 1, 1, 65000.00, 12, 'Alimento seco completo para perros adultos de raza grande.'),
('Pipeta Antipulgas Revolution Plus', 2, 2, 14500.00, 25, 'Protección contra pulgas, garrapatas y ácaros.'),
('Collar Ajustable Reflectivo Flúor', 3, 3, 8500.00, 18, 'Collar de alta resistencia y visibilidad nocturna.'),
('Shampoo Hipoalergénico con Aloe Vera', 2, 2, 9200.00, 4, 'Cuidado de piel sensible. Alerta: bajo stock.');

-- Datos de Veterinarios y Turnos
INSERT INTO veterinarios (nombre, apellido, matricula, especialidad) VALUES 
('Dra. Laura', 'Fernández', 'MN-4458', 'Clínica General y Vacunación'),
('Dr. Pablo', 'Rossi', 'MN-6712', 'Cirugía y Traumatología');

INSERT INTO turnos (id_mascota, id_veterinario, fecha_hora, motivo, estado) VALUES 
(1, 1, DATE_ADD(NOW(), INTERVAL 1 DAY), 'Control anual y vacuna séxtuple', 'Pendiente'),
(2, 1, DATE_ADD(NOW(), INTERVAL 2 DAY), 'Consulta médica por decaimiento', 'Pendiente'),
(3, 2, DATE_ADD(NOW(), INTERVAL 3 DAY), 'Chequeo dermatológico y peso', 'Pendiente');

-- ============================================================================
-- 4. CONSULTA DE DEMOSTRACIÓN (INNER JOIN)
-- ============================================================================
-- SELECT 
--     m.id_mascota AS ID,
--     m.nombre AS Mascota,
--     e.nombre AS Especie,
--     CONCAT(d.nombre, ' ', d.apellido) AS Dueño,
--     d.telefono AS Telefono,
--     d.email AS Email,
--     m.raza AS Raza,
--     m.edad AS Edad,
--     m.peso AS Peso_Kg
-- FROM mascotas m
-- INNER JOIN especies e ON m.id_especie = e.id_especie
-- INNER JOIN duenios d ON m.id_duenio = d.id_duenio
-- ORDER BY m.id_mascota DESC;
