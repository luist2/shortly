# Shortly

Un acortador de URLs moderno y eficiente construido con .NET y Angular.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-15-DD0031?logo=angular)](https://angular.io/)

## Live Demo

- **Frontend (Angular + Netlify)**  
  https://shortly-platform.netlify.app

- **Backend API (.NET + Render)**  
  https://shortly-api-nulu.onrender.com

> La API puede tardar unos segundos en responder la primera vez debido al cold start de Render.

---

## Características Principales

- **Acortamiento de URLs** con códigos únicos altamente eficientes.
- **Expiración Personalizable**: Opciones para definir la vida útil de un enlace (1 hora, 1 día, 1 semana, etc.).
- **Panel de Control (Dashboard)**: Interfaz intuitiva para gestionar URLs y ver analíticas/estadísticas de clicks en tiempo real.
- **Seguridad y Autenticación**: Sistema robusto con JWT, rotación de tokens y almacenamiento en cookies HTTP-Only.
- **Rendimiento**: API RESTful fuertemente tipada, paginada, y protegida contra abusos mediante Rate Limiting.

## Stack

- **Backend:** .NET 8 • Entity Framework Core • PostgreSQL • JWT
- **Frontend:** Angular 15

## Instalación

### Requisitos

- .NET 8 SDK
- PostgreSQL
- Node.js 18+
- Angular CLI (`npm install -g @angular/cli`)

### Backend (.NET)
```bash
# Clonar repositorio
git clone https://github.com/luist2/shortly.git
cd shortly/Backend/Shortly_API

# Crear archivo de configuración local
cp appsettings.json appsettings.Development.json
```

**Configuración requerida en `appsettings.Development.json`**:
Debes configurar los bloques críticos como tu base de datos y la llave secreta del JWT:

```json
{
  "JwtSettings": {
    "Token": "TU_SUPER_SECRETO_JWT_AQUI_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "Shortly.API",
    "Audience": "Shortly.Client",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7,
    "RefreshTokenRotationHours": 24
  },
  "GeneralSettings": {
    "BaseDomain": "https://localhost:7161", 
    "DefaultRole": "User"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=shortly_db;Username=TU_USUARIO;Password=TU_PASSWORD"
  }
}
```

```bash
# Instalar paquetes dependencias
dotnet restore

# Ejecutar migraciones a la DB y levantar la API
dotnet ef database update
dotnet run
```

Accede a `https://localhost:7161/scalar` para revisar la documentación.

### Frontend (Angular)

> **Nota de Entorno**: Por defecto, el frontend asume que tu API de desarrollo corre en `https://localhost:7161`. Si tu servidor backend asigna un puerto distinto, asegúrate de actualizar la propiedad `apiUrl` en el archivo `src/environments/environment.ts`.

```bash
# Ir al directorio del frontend
cd shortly/Frontend/shortly-frontend

# Instalar los paquetes Node
npm install

# Levantar aplicación en servidor de desarrollo
ng serve
```

La aplicación estará disponible localmente en `http://localhost:4200`.

---

## Arquitectura y Patrones del Backend

La API de backend (.NET 8) está estructurada para priorizar escalabilidad, seguridad y limpieza de código:

- **Patrón Repositorio**: Capa de abstracción para el acceso a datos (EF Core / PostgreSQL) facilitando el mantenimiento y las pruebas automatizadas.
- **Inyección de Dependencias (DI)**: Ampliamente implementada para mantener un bajo acoplamiento entre servicios (ej. `IAuthService`, `IUrlShortenerService`).
- **Seguridad Moderna**: 
  - Almacenamiento del *Refresh Token* mitigando ataques XSS mediante el uso de cookies **HTTP-Only** configuradas con opciones estrictas (`SameSite=None`, `Secure`).
  - Protección de endpoints de creación contra ataques de fuerza bruta utilizando el middleware de **Rate Limiting** nativo de .NET 8.
- **Rendimiento**: Respuestas paginadas desde la base de datos limitando sobrecargas en memoria, y gestión de configuraciones segregadas por dominio mediante el patrón `Options`.

## Endpoints principales

**Autenticación**
```http
POST   /api/auth/register               # Registro de usuario
POST   /api/auth/login                  # Inicio de sesión
POST   /api/auth/refresh-tokens         # Refrescar tokens
POST   /api/auth/logout                 # Cerrar sesión
```

**Gestión de URLs**
```http
POST   /api/urlshortener/urls           # Crear URL corta
GET    /api/urlshortener/urls           # Listar URLs del usuario
GET    /api/urlshortener/urls/{code}    # Obtener estadísticas
DELETE /api/urlshortener/urls/{code}    # Eliminar URL
```

**Redirección**
```http
GET    /{shortCode}                     # Redirección a URL original
```

## Autor

**luist2** • [LinkedIn](https://linkedin.com/in/luis-troncoso-ulloa-4b1481326)