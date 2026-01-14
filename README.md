# Shortly 🔗

Un acortador de URLs moderno y eficiente construido con .NET y Angular.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-15-DD0031?logo=angular)](https://angular.io/)

## 🔴 Live Demo

- **Frontend (Angular + Netlify)**  
  https://shortly-platform.netlify.app

- **Backend API (.NET + Render)**  
  https://shortly-api-nulu.onrender.com

> La API puede tardar unos segundos en responder la primera vez debido al cold start de Render.

---

## Características

- Acortamiento de URLs con códigos únicos
- Autenticación con JWT
- Estadísticas de clicks
- API RESTful documentada

## Stack

- **Backend:** .NET 8 • Entity Framework Core • PostgreSQL • JWT
- **Frontend:** Angular 15

## Instalación

### Requisitos
- .NET 8 SDK
- PostgreSQL

### Setup

```bash
# Clonar
git clone https://github.com/luist2/shortly.git
cd shortly/backend/Shortly_API

# Configurar
cp appsettings.json appsettings.Development.json
# Edita appsettings.Development.json con tu JWT secret y connection string

# Instalar dependencias
dotnet restore

# Ejecutar
dotnet ef database update
dotnet run
```

Accede a `https://localhost:7161/scalar` para revisar la documentación

### Endpoints principales

**Autenticación**

```http
POST   /api/auth/register               # Registro de usuario
POST   /api/auth/login                  # Inicio de sesión
POST   /api/auth/refresh-tokens         # Refrescar tokens
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

## 👤 Autor

**luist2** • [LinkedIn](https://linkedin.com/in/luis-troncoso-ulloa-4b1481326)