# 🐾 VetWeb — Plataforma Veterinaria & Pet Shop con IA

Sistema web integral para clínica veterinaria y pet shop desarrollado en **C# con ASP.NET Core MVC (.NET 10)** y base de datos relacional **MariaDB / MySQL**. Incluye asistente virtual inteligente (**VetBot**) integrado con la API de **Google Gemini**.

---

## 🚀 Características Principales

- **🐱 Directorio de Pacientes (ABM):** Gestión de mascotas, especies y dueños relacionados con claves foráneas (FK) y consultas estructuradas mediante `INNER JOIN` (Entity Framework Core).
- **📅 Módulo de Turnos Médicos:** Formulario de reserva online para consultas veterinarias, vacunación y cirugías con validación de servidor.
- **🛒 Pet Shop Oficial & Carrito de Compras:** Catálogo categorizado de alimentos balanceados, accesorios y cuidado pet con carrito interactivo (Offcanvas) y flujo de checkout con transferencia bancaria (Alias `vetweb.oficial`).
- **🤖 VetBot (Asistente con Inteligencia Artificial):** Widget flotante interactivo en el frontend conectado a un endpoint seguro en el backend (`/api/chat`) que consulta a **Google Gemini (1.5 Flash)**. Las API Keys permanecen 100% privadas en el servidor y nunca se exponen al cliente.
- **🎨 Diseño Moderno:** Interfaz responsiva con Bootstrap 5, paleta purple-violet (`#7c3aed`), soporte móvil y notificaciones.

---

## 🛠️ Tecnologías Utilizadas

- **Lenguaje:** C# (.NET 10)
- **Framework Web:** ASP.NET Core MVC
- **ORM:** Entity Framework Core (Pomelo MySQL Connector 9.0)
- **Base de Datos:** MariaDB / MySQL 8.4 (InnoDB)
- **Frontend:** HTML5, CSS3, JavaScript vanilla, Bootstrap 5, Bootstrap Icons
- **IA:** Google Gemini REST API (`gemini-1.5-flash`)

---

## 📋 Requisitos Previos

1. **.NET 10 SDK** instalado ([Descargar .NET](https://dotnet.microsoft.com/download)).
2. **Servidor MySQL / MariaDB** corriendo en el puerto `3306` (ej: Laragon, XAMPP o MySQL Server).
3. *(Opcional)* Clave de API gratuita de [Google AI Studio](https://aistudio.google.com/) para activar VetBot.

---

## ⚙️ Puesta en Marcha

### 1. Clonar el repositorio
```bash
git clone https://github.com/Jvier-Zaia/vetweb.git
cd vetweb
```

### 2. Crear y poblar la Base de Datos
Ejecutar el script `veterinaria_db.sql` en HeidiSQL, MySQL Workbench o desde la terminal:
```bash
mysql -u root -p < veterinaria_db.sql
```

### 3. Configurar la Cadena de Conexión
Si tu servidor MySQL tiene contraseña en `root`, edita `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=veterinaria_db;User=root;Password=TU_PASSWORD;Charset=utf8mb4;"
}
```

### 4. Configurar la API Key de Gemini (VetBot)
Para que VetBot responda con inteligencia artificial, puedes configurar tu clave de dos formas:

- **Opción A (Archivo local seguro - Recomendado):**
  Crea un archivo `appsettings.Local.json` en la raíz del proyecto (este archivo está ignorado en `.gitignore` y nunca se subirá a Git):
  ```json
  {
    "Gemini": {
      "ApiKey": "TU_GEMINI_API_KEY_AQUI"
    }
  }
  ```

- **Opción B (Variable de Entorno):**
  ```bash
  # En PowerShell:
  $env:GEMINI_API_KEY="TU_GEMINI_API_KEY_AQUI"

  # En CMD:
  set GEMINI_API_KEY=TU_GEMINI_API_KEY_AQUI
  ```

### 5. Compilar y Ejecutar
```bash
dotnet run
```
Abre tu navegador en `http://localhost:5096` (o la dirección que indique la consola).

---

## 📂 Estructura del Proyecto

```text
VetWeb/
├── Controllers/            # Controladores MVC (Mascotas, Turnos, Tienda, Chat)
├── Data/                   # DbContext (Entity Framework Core)
├── Models/                 # Entidades y ViewModels (Mascota, Dueño, Especie, Chat)
├── Views/                  # Vistas Razor (.cshtml) con diseño Purple Bootstrap 5
│   ├── Mascotas/           # ABM y vistas de pacientes
│   ├── Tienda/             # Catálogo de productos y checkout
│   ├── Turnos/             # Reserva y agenda de turnos
│   └── Shared/_Layout.cshtml # Navbar, Carrito Offcanvas y Widget de VetBot
├── veterinaria_db.sql      # Script de estructura y datos de prueba
├── appsettings.json        # Configuración general
├── appsettings.Local.json  # Configuración privada (ignorado por Git)
└── Program.cs              # Punto de entrada y configuración de servicios
```

---



*Programación 2 — Trabajo Práctico Integrador (Noviembre 2026)*
