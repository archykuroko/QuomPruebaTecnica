# Quom Asset Management - Backend

API REST para la gestión y resguardo de activos de TI. La solución administra activos, colaboradores, proveedores, asignaciones, devoluciones e historial de movimientos, con autenticación JWT, autorización por roles y persistencia en Microsoft SQL Server mediante ADO.NET y procedimientos almacenados.

Este directorio contiene la solución de backend y su proyecto de pruebas automatizadas.

## Contenido

- [Alcance](#alcance)
- [Arquitectura](#arquitectura)
- [Tecnologías](#tecnologías)
- [Requisitos](#requisitos)
- [Configuración](#configuración)
- [Base de datos](#base-de-datos)
- [Ejecución](#ejecución)
- [Autenticación y autorización](#autenticación-y-autorización)
- [Endpoints](#endpoints)
- [Reglas de negocio](#reglas-de-negocio)
- [Paginación y filtros](#paginación-y-filtros)
- [Auditoría e historial](#auditoría-e-historial)
- [Manejo de errores](#manejo-de-errores)
- [Concurrencia](#concurrencia)
- [Pruebas automatizadas](#pruebas-automatizadas)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Decisiones técnicas](#decisiones-técnicas)
- [Tiempo de desarrollo](#tiempo-de-desarrollo)
- [Uso de asistencia de IA](#uso-de-asistencia-de-ia)
- [Notas de entrega](#notas-de-entrega)

## Alcance

El backend cubre los siguientes casos de uso:

- Inicio de sesión por nombre de usuario o correo electrónico.
- Generación y validación de tokens JWT.
- Autorización con roles `Administrador` y `Operador`.
- Consulta individual y búsqueda paginada de activos.
- Alta y actualización de activos.
- Asignación de activos a colaboradores.
- Devolución de activos y registro de su condición.
- Consulta del historial de movimientos de cada activo.
- Consulta y alta de colaboradores.
- Consulta y alta de proveedores.
- Asociación de proveedores con uno o más tipos de servicio.
- Consulta del catálogo de tipos de servicio.
- Auditoría de altas, cambios, asignaciones y devoluciones.
- Protección de operaciones críticas ante solicitudes concurrentes.
- Validaciones de entrada y respuestas de error uniformes.

## Arquitectura

La solución sigue una separación por responsabilidades:

```text
HTTP / Controllers
        |
        v
Services (reglas de aplicación y negocio)
        |
        v
Repositories (ADO.NET y mapeo de datos)
        |
        v
Stored Procedures
        |
        v
Microsoft SQL Server
```

- **Controllers:** exponen los recursos HTTP, aplican autenticación/autorización y obtienen la identidad del usuario autenticado.
- **Services:** validan reglas del dominio antes de acceder a datos.
- **Repositories:** encapsulan las conexiones, parámetros SQL, ejecución de procedimientos almacenados y mapeo de resultados.
- **DTOs:** definen los contratos de entrada y aplican validaciones declarativas.
- **Models:** representan las entidades y resultados utilizados por la aplicación.
- **Middleware:** centraliza la traducción de excepciones a respuestas HTTP.
- **Security:** genera tokens JWT con identidad y rol.
- **SQL Server:** protege la integridad mediante procedimientos, transacciones, restricciones, relaciones e índices.

La API no utiliza Entity Framework Core. El acceso a datos se realiza con `Microsoft.Data.SqlClient` y parámetros tipados.

## Tecnologías

- .NET 8
- ASP.NET Core Web API
- Microsoft SQL Server / LocalDB
- ADO.NET (`Microsoft.Data.SqlClient`)
- JWT Bearer Authentication
- ASP.NET Core Identity `PasswordHasher<TUser>`
- Swagger / OpenAPI mediante Swashbuckle
- xUnit
- Moq
- Microsoft Test SDK
- coverlet collector

## Requisitos

- .NET 8 SDK.
- Microsoft SQL Server o SQL Server LocalDB.
- SQL Server Management Studio, Azure Data Studio o una herramienta equivalente para restaurar el respaldo y ejecutar scripts.
- Visual Studio 2022 es opcional; también puede utilizarse la CLI de .NET.

## Configuración

La configuración principal se encuentra en:

```text
Quom.AssetManagement.Api/appsettings.json
```

La aplicación espera las siguientes claves:

```json
{
  "ConnectionStrings": {
    "Db": "Server=(localdb)\\MSSQLLocalDB;Database=Quom;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "REEMPLAZAR_POR_UN_SECRETO_SEGURO",
    "Issuer": "Quom.AssetManagement.Api",
    "Audience": "Quom.AssetManagement.Client",
    "ExpirationMinutes": 60
  }
}
```

Para una instancia distinta de LocalDB, se debe modificar `ConnectionStrings:Db`. Ejemplo con una instancia local de SQL Server:

```text
Server=localhost;Database=Quom;Trusted_Connection=True;TrustServerCertificate=True;
```

La clave JWT incluida en el repositorio es exclusivamente de desarrollo. No debe reutilizarse en producción. Para una instalación real se recomienda proporcionar `Jwt:Key` mediante variables de entorno, Secret Manager o un almacén de secretos:

```powershell
$env:Jwt__Key = "una-clave-larga-aleatoria-y-privada"
```

## Base de datos

Los artefactos de base de datos se entregan fuera del directorio del backend:

```text
Database/
|-- Backup/
|   `-- Quom.bak
|-- Seed/
|   `-- SeedData.sql
`-- README.md
```

### Estrategia de entrega

- `Quom.bak` es el mecanismo principal de restauración. Contiene el esquema completo, procedimientos almacenados, restricciones, índices, usuarios demo y datos existentes de desarrollo/demostración.
- `SeedData.sql` agrega o normaliza un catálogo de demostración reconocible. Está diseñado para ser idempotente: puede ejecutarse más de una vez sin duplicar los registros identificados por sus claves de negocio.
- `Database/README.md` documenta con mayor detalle el modelo relacional, restricciones, índices, procedimientos y pruebas manuales de SQL Server.

### Restaurar el respaldo

1. Abrir SQL Server Management Studio.
2. Seleccionar **Databases > Restore Database**.
3. Elegir **Device** y agregar `Database/Backup/Quom.bak`.
4. Restaurar la base con el nombre `Quom`.
5. Si SQL Server propone rutas de archivos que no existen en el equipo destino, corregirlas en la página **Files** antes de restaurar.
6. Confirmar que la cadena de conexión de la API apunta a la instancia donde se restauró la base.

Ejemplo equivalente en T-SQL, ajustando las rutas lógicas y físicas a la instancia destino:

```sql
RESTORE FILELISTONLY
FROM DISK = 'C:\ruta\Quom.bak';

RESTORE DATABASE [Quom]
FROM DISK = 'C:\ruta\Quom.bak'
WITH
    MOVE 'Quom' TO 'C:\ruta-sql\Quom.mdf',
    MOVE 'Quom_log' TO 'C:\ruta-sql\Quom_log.ldf',
    REPLACE,
    RECOVERY;
```

Los nombres lógicos devueltos por `RESTORE FILELISTONLY` son la fuente correcta; deben reemplazarse en el segundo comando si difieren del ejemplo.

### Ejecutar el seed

Después de restaurar `Quom.bak`, ejecutar el archivo completo:

```text
Database/Seed/SeedData.sql
```

El script utiliza `IF NOT EXISTS` y los procedimientos almacenados de la solución cuando corresponde. Incluye:

- Roles `Administrador` y `Operador`.
- Usuarios demo.
- Tipos de servicio `Compra`, `Mantenimiento` y `Arrendamiento`.
- Cuatro colaboradores de distintas áreas y ubicaciones.
- Proveedores con diferentes combinaciones de servicios.
- Ocho activos `DEMO-TI-001` a `DEMO-TI-008` con categorías, estados y tipos de propiedad variados.
- Una asignación inicial para disponer de trazabilidad desde el primer uso.

El seed presupone que el usuario `admin` está presente en el respaldo, porque lo utiliza como responsable de las operaciones auditadas.

### Modelo e integridad

Las tablas principales son:

- `Roles`
- `Users`
- `Employees`
- `Suppliers`
- `ServiceTypes`
- `SupplierServices`
- `Assets`
- `AssetAssignments`
- `AssetMovements`

Entre las protecciones de integridad se incluyen:

- código de activo único;
- número de serie único cuando no es nulo;
- nombre de usuario y correo únicos;
- claves foráneas entre entidades;
- restricciones para estados y tipos de propiedad;
- proveedor y fecha de término obligatorios para activos arrendados;
- una sola asignación activa por activo mediante índice único filtrado;
- conservación de asignaciones y movimientos históricos.

### Procedimientos almacenados utilizados por la API

```text
usp_Assets_GetById
usp_Assets_Search
usp_Assets_Create
usp_Assets_Update
usp_Assets_Assign
usp_Assets_Return
usp_Assets_GetHistory

usp_Employees_GetAll
usp_Employees_Create

usp_Suppliers_GetAll
usp_Suppliers_Create
usp_Suppliers_GetServices
usp_Suppliers_SetServices

usp_ServiceTypes_GetAll

usp_Users_GetByLogin
usp_Users_RegisterFailedLogin
usp_Users_ResetLoginAttempts
```

## Ejecución

Desde `Backend/Quom.AssetManagement`:

```powershell
dotnet restore
dotnet build
dotnet run --project .\Quom.AssetManagement.Api\Quom.AssetManagement.Api.csproj
```

En el perfil HTTPS incluido, Swagger queda disponible en:

```text
https://localhost:7201/swagger
```

El perfil HTTP utiliza:

```text
http://localhost:5149/swagger
```

Swagger se habilita únicamente cuando `ASPNETCORE_ENVIRONMENT` es `Development`.

Si el certificado HTTPS de desarrollo no está configurado:

```powershell
dotnet dev-certs https --trust
```

### Verificación rápida

1. Restaurar `Quom.bak`.
2. Ajustar la cadena de conexión y la clave JWT.
3. Iniciar la API.
4. Ejecutar `POST /api/auth/login` con un usuario demo.
5. Copiar el token devuelto.
6. En Swagger, seleccionar **Authorize** e ingresar el token.
7. Consultar `GET /api/assets`.
8. Probar búsqueda, detalle e historial.
9. Con `admin`, probar alta o actualización.
10. Probar asignación y devolución con un activo disponible.
11. Ejecutar la suite automatizada.

## Autenticación y autorización

El endpoint de inicio de sesión acepta nombre de usuario o correo electrónico:

```http
POST /api/auth/login
Content-Type: application/json

{
  "login": "admin",
  "password": "Admin123!"
}
```

Respuesta satisfactoria:

```json
{
  "token": "<jwt>",
  "expiresAt": "<fecha-UTC>",
  "userId": 1,
  "username": "admin",
  "role": "Administrador"
}
```

El JWT contiene los claims de identificador de usuario, nombre de usuario y rol. Se firma con HMAC-SHA256 y se valida por emisor, audiencia, vigencia y clave de firma. La tolerancia adicional de expiración está deshabilitada (`ClockSkew = 0`).

Para consumir endpoints protegidos:

```http
Authorization: Bearer <token>
```

### Roles

| Operación | Administrador | Operador |
|---|:---:|:---:|
| Consultar activos e historial | Sí | Sí |
| Asignar y devolver activos | Sí | Sí |
| Consultar colaboradores y proveedores | Sí | Sí |
| Consultar tipos de servicio | Sí | Sí |
| Crear o actualizar activos | Sí | No |
| Crear colaboradores | Sí | No |
| Crear proveedores | Sí | No |
| Configurar servicios de proveedores | Sí | No |

### Bloqueo por intentos fallidos

La autenticación rechaza usuarios inexistentes, inactivos o con bloqueo vigente. Una contraseña incorrecta registra un intento fallido; una autenticación correcta restablece el contador. La política configurada en la base de datos bloquea temporalmente la cuenta después de cinco intentos fallidos durante 15 minutos.

El mensaje de credenciales inválidas es deliberadamente genérico para no revelar si una cuenta existe, está inactiva o está bloqueada.

### Usuarios demo

| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | `admin` | `Admin123!` |
| Operador | `operador` | `Operador123!` |

Estas credenciales son exclusivamente para evaluación local. Deben eliminarse o reemplazarse en cualquier despliegue real.

## Endpoints

Todos los endpoints, excepto el inicio de sesión, requieren JWT.

### Autenticación

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | Autentica por usuario o correo y devuelve un JWT. |

### Activos

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/assets` | Autenticado | Busca activos con filtros y paginación. |
| `GET` | `/api/assets/{id}` | Autenticado | Obtiene el detalle de un activo. |
| `POST` | `/api/assets` | Administrador | Crea un activo. Todo activo creado por la API inicia como `Disponible`. |
| `PUT` | `/api/assets/{id}` | Administrador | Actualiza los datos permitidos de un activo. |
| `POST` | `/api/assets/{id}/assign` | Autenticado | Asigna un activo disponible a un colaborador activo. |
| `POST` | `/api/assets/{id}/return` | Autenticado | Devuelve un activo asignado y registra su condición. |
| `GET` | `/api/assets/{id}/history` | Autenticado | Consulta la bitácora del activo. |

Ejemplo de creación:

```json
{
  "assetCode": "TI-000100",
  "serialNumber": "SN-000100",
  "category": "Laptop",
  "brand": "Dell",
  "model": "Latitude 5440",
  "ownershipType": "Propio",
  "supplierId": null,
  "currentLocation": "Almacén TI",
  "purchaseDate": "2026-08-01",
  "rentalEndDate": null
}
```

Ejemplo de asignación:

```json
{
  "employeeId": 1,
  "notes": "Equipo entregado con cargador."
}
```

Ejemplo de devolución:

```json
{
  "returnCondition": "Buen estado",
  "notes": "Sin daños visibles."
}
```

### Colaboradores

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/employees` | Autenticado | Obtiene colaboradores. |
| `POST` | `/api/employees` | Administrador | Registra un colaborador. |

### Proveedores

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/suppliers` | Autenticado | Obtiene proveedores. |
| `POST` | `/api/suppliers` | Administrador | Registra un proveedor. |
| `GET` | `/api/suppliers/{id}/services` | Autenticado | Obtiene los servicios asociados al proveedor. |
| `PUT` | `/api/suppliers/{id}/services` | Administrador | Reemplaza la selección de tipos de servicio del proveedor. |

Ejemplo para configurar servicios:

```json
{
  "serviceTypeIds": [1, 2]
}
```

### Tipos de servicio

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/service-types` | Autenticado | Obtiene el catálogo de tipos de servicio. |

## Reglas de negocio

### Activos

- `AssetCode`, categoría, marca y tipo de propiedad son obligatorios.
- `AssetCode` no puede repetirse.
- El número de serie, cuando existe, debe ser único.
- Los tipos de propiedad válidos son `Propio` y `Arrendado`.
- Un activo arrendado requiere proveedor y fecha de término de arrendamiento.
- Los estados admitidos son `Disponible`, `Asignado`, `Mantenimiento` y `Retirado`.
- Los activos creados mediante la API inician como `Disponible`.
- `Asignado` no puede establecerse directamente con `PUT`; sólo el flujo de asignación puede producir ese estado.
- Un activo `Retirado` no puede reactivarse.

### Asignación

- El activo y el colaborador deben existir.
- El colaborador debe estar activo.
- Sólo puede asignarse un activo `Disponible`.
- Un activo no puede tener más de una asignación activa.
- La operación cambia el activo a `Asignado`, crea la asignación y registra el movimiento en una sola transacción.
- El usuario que ejecuta la acción se toma del JWT, no del cuerpo de la solicitud.

### Devolución

- Sólo puede devolverse un activo actualmente asignado.
- Debe existir una asignación activa.
- La condición de devolución es obligatoria.
- La operación cierra la asignación sin eliminarla, cambia el activo a `Disponible` y registra un movimiento `Returned`.

### Proveedores

- Un proveedor puede ofrecer varios tipos de servicio.
- La actualización de servicios exige al menos un identificador.
- La relación se transmite al procedimiento almacenado como JSON parametrizado.

## Paginación y filtros

`GET /api/assets` admite:

| Parámetro | Tipo | Predeterminado | Descripción |
|---|---|---:|---|
| `search` | texto | — | Coincidencia parcial por código, número de serie, marca o modelo. |
| `status` | texto | — | Filtra por un estado válido. |
| `category` | texto | — | Filtra por categoría. |
| `pageNumber` | entero | `1` | Página solicitada; mínimo 1. |
| `pageSize` | entero | `10` | Registros por página; entre 1 y 100. |

Ejemplo:

```http
GET /api/assets?search=Dell&status=Disponible&category=Laptop&pageNumber=1&pageSize=10
```

El procedimiento `usp_Assets_Search` utiliza `OFFSET`/`FETCH` y devuelve dos conjuntos de resultados: los elementos de la página y `TotalRecords`. La API responde con una estructura equivalente a:

```json
{
  "items": [],
  "totalRecords": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

## Auditoría e historial

La trazabilidad se conserva en dos niveles:

- `AssetAssignments` mantiene el historial de asignaciones. Una devolución completa `ReturnedAt`, `ReturnedByUserId` y `ReturnCondition`; no elimina el registro.
- `AssetMovements` funciona como bitácora general de activos y registra eventos como creación, asignación, devolución, cambios de estado y cambios de ubicación.

Los movimientos pueden conservar estado anterior/nuevo, ubicación anterior/nueva, usuario responsable, fecha y observaciones. Las altas y actualizaciones reciben desde la API el identificador del usuario autenticado para atribuir correctamente la operación.

## Manejo de errores

`ExceptionMiddleware` proporciona un formato uniforme:

```json
{
  "statusCode": 409,
  "message": "Descripción controlada del error."
}
```

Las respuestas principales son:

- `400 Bad Request`: validación de DTO, argumento inválido o regla de negocio aplicable.
- `401 Unauthorized`: credenciales inválidas, token ausente/inválido o identidad no recuperable.
- `403 Forbidden`: usuario autenticado sin el rol requerido.
- `404 Not Found`: activo, colaborador u otro recurso solicitado no encontrado.
- `409 Conflict`: duplicados, activo no disponible, asignación ya activa o devolución incompatible con el estado actual.
- `500 Internal Server Error`: error inesperado o de SQL Server no traducido; el detalle interno se registra, pero no se expone al cliente.

Las validaciones declarativas de ASP.NET Core devuelven:

```json
{
  "statusCode": 400,
  "message": "Existen errores de validación.",
  "errors": {
    "campo": ["mensaje de validación"]
  }
}
```

Los repositorios usan parámetros tipados y no concatenan entradas del usuario en comandos SQL.

## Concurrencia

La asignación y la devolución están protegidas en SQL Server porque afectan varias tablas y pueden recibir solicitudes simultáneas.

La estrategia combina:

1. Transacciones para confirmar o revertir la operación completa.
2. Bloqueos `UPDLOCK` y `HOLDLOCK` sobre el activo durante la validación y modificación.
3. Revalidación del estado dentro de la transacción.
4. Índice único filtrado `UX_AssetAssignments_ActiveAsset`, que impide más de una asignación con `ReturnedAt IS NULL` para el mismo activo.

Esta protección evita depender exclusivamente de una comprobación realizada desde la API. El comportamiento fue validado manualmente con dos sesiones de SQL Server: la segunda operación esperó la liberación del bloqueo y, al continuar, observó el estado actualizado del activo.

## Pruebas automatizadas

El proyecto `Quom.AssetManagement.Tests` utiliza xUnit y Moq. La suite final contiene **20 pruebas: 20 superadas, 0 errores y 0 omitidas**.

Distribución:

- **15 pruebas de `AssetService`:** actualización, reglas de propiedad y estados, asignación, devolución, búsqueda y límite de paginación.
- **5 pruebas de `AuthService`:** usuario inexistente, usuario inactivo, usuario bloqueado, contraseña incorrecta y autenticación correcta con generación de JWT.

Los servicios se prueban aislando el acceso a datos mediante mocks. En autenticación, `JwtTokenService` genera un token real con configuración de prueba en memoria.

Para ejecutar la suite desde `Backend/Quom.AssetManagement`:

```powershell
dotnet test .\Quom.AssetManagement.Tests\Quom.AssetManagement.Tests.csproj
```

Para obtener cobertura:

```powershell
dotnet test .\Quom.AssetManagement.Tests\Quom.AssetManagement.Tests.csproj --collect:"XPlat Code Coverage"
```

Además de la suite automatizada, se realizaron pruebas manuales sobre los procedimientos almacenados para altas, actualización, asignación, rechazo de asignación duplicada, devolución, rechazo de devolución duplicada, auditoría y concurrencia.

## Estructura del proyecto

```text
Backend/
`-- Quom.AssetManagement/
    |-- Quom.AssetManagement.slnx
    |-- Quom.AssetManagement.Api/
    |   |-- Controllers/
    |   |-- Data/
    |   |-- DTOs/
    |   |-- Middleware/
    |   |-- Models/
    |   |-- Repositories/
    |   |   |-- Interfaces/
    |   |   `-- Implementations/
    |   |-- Security/
    |   |-- Services/
    |   |   |-- Interfaces/
    |   |   `-- Implementations/
    |   |-- Properties/
    |   |-- Program.cs
    |   `-- appsettings.json
    `-- Quom.AssetManagement.Tests/
        |-- Services/
        |   |-- AssetServiceTests.cs
        |   `-- AuthServiceTests.cs
        `-- Quom.AssetManagement.Tests.csproj
```

## Decisiones técnicas

### ADO.NET y procedimientos almacenados

Se eligió ADO.NET para mantener explícito el contrato con SQL Server y utilizar los procedimientos almacenados como frontera de persistencia. Esto permite controlar tipos, conjuntos de resultados, transacciones, bloqueos y errores SQL sin introducir un ORM adicional.

### Reglas en más de una capa

Las validaciones de formato y tamaño se realizan en DTOs; las reglas de aplicación se comprueban en Services; y las invariantes críticas se vuelven a proteger en SQL Server. Esta defensa en profundidad evita que la integridad dependa de un único punto.

### Separación entre usuarios y colaboradores

`Users` representa cuentas con acceso al sistema. `Employees` representa personas que reciben activos. Separar ambos conceptos evita exigir que cada colaborador tenga credenciales y mantiene claro quién ejecutó una operación frente a quién recibió un activo.

### Asignación y devolución como operaciones explícitas

El estado `Asignado` no se permite mediante una actualización genérica. Los endpoints `/assign` y `/return` expresan la intención del negocio, preservan el historial y ejecutan los cambios relacionados de forma transaccional.

### Historial inmutable

Las devoluciones cierran asignaciones en lugar de eliminarlas. Asimismo, los movimientos se conservan como bitácora. Esta decisión prioriza trazabilidad y auditoría.

### Autorización declarativa

Las políticas de acceso se expresan con `[Authorize]` y `[Authorize(Roles = "Administrador")]` en los controladores. El rol viaja como claim firmado dentro del JWT.

### Errores controlados

El middleware global evita duplicar bloques de manejo de excepciones y traduce errores técnicos o de negocio a códigos HTTP consistentes, sin exponer detalles internos de SQL Server.

### Seed idempotente y respaldo completo

El respaldo ofrece una puesta en marcha rápida y fiel; el seed permite reconocer, reponer y ampliar datos de demostración sin duplicarlos. Los códigos `DEMO-TI-*` distinguen el catálogo preparado para evaluación de los registros históricos generados durante el desarrollo.

## Tiempo de desarrollo

El backend, la capa de datos, las pruebas y la preparación de entrega se realizaron en varias sesiones de trabajo. El esfuerzo efectivo estimado fue de **aproximadamente 18 a 24 horas**, distribuido de forma orientativa así:

- análisis del requerimiento y diseño de datos: 3 a 4 horas;
- modelo SQL, restricciones, índices y procedimientos: 6 a 8 horas;
- API, autenticación, autorización y manejo de errores: 5 a 6 horas;
- pruebas manuales y automatizadas: 3 a 4 horas;
- seed, respaldo, revisión final y documentación: 2 a 3 horas.

La estimación representa tiempo de trabajo efectivo y no tiempo calendario. Incluye iteraciones de diseño, depuración y validación.

## Uso de asistencia de IA

Durante el desarrollo se utilizó ChatGPT de OpenAI como herramienta de apoyo. Su participación se concentró en:

- revisión del modelo relacional, restricciones e índices;
- razonamiento sobre reglas de negocio y casos límite;
- comparación de alternativas para concurrencia y trazabilidad;
- revisión de firmas, contratos y separación de responsabilidades;
- propuesta de escenarios de prueba;
- apoyo durante la depuración de errores de C# y SQL;
- revisión de seguridad, configuración y preparación de entrega;
- organización y redacción de la documentación técnica.

La asistencia no sustituyó la responsabilidad del desarrollador. Las decisiones finales, la integración del código y la validación del comportamiento fueron realizadas por el desarrollador mediante revisión de las propuestas, ejecución de la aplicación, pruebas directas de procedimientos almacenados, inspección de datos, prueba manual de concurrencia y ejecución de la suite automatizada.

Las sugerencias se ajustaron o descartaron cuando no coincidían con la implementación real. El objetivo del uso de IA fue acelerar la revisión y ampliar el análisis de alternativas, manteniendo la autoría, el criterio técnico y la validación final en el desarrollador.

## Notas de entrega

- El respaldo principal se encuentra en `Database/Backup/Quom.bak`.
- El catálogo reproducible se encuentra en `Database/Seed/SeedData.sql`.
- La documentación detallada de SQL Server se encuentra en `Database/README.md`.
- El respaldo conserva tanto el catálogo `DEMO-TI-*` como registros históricos de desarrollo, de forma intencional, para mostrar los flujos y la trazabilidad acumulada.
- Swagger requiere entorno `Development`.
- La clave JWT del repositorio y las credenciales demo son sólo para evaluación local.
- Antes de una entrega pública o despliegue real deben rotarse las credenciales, externalizarse los secretos y ajustarse la cadena de conexión.
- No se incluyen artefactos compilados como `bin` u `obj` como parte funcional de la entrega.

### Checklist de evaluación

- [x] API REST en ASP.NET Core 8.
- [x] Persistencia en SQL Server con ADO.NET.
- [x] Procedimientos almacenados para las operaciones principales.
- [x] Autenticación JWT.
- [x] Roles Administrador y Operador.
- [x] Alta, consulta, actualización, asignación y devolución de activos.
- [x] Colaboradores, proveedores y tipos de servicio.
- [x] Paginación y filtros.
- [x] Historial y auditoría.
- [x] Validaciones y manejo global de errores.
- [x] Protección de concurrencia para asignaciones.
- [x] Respaldo completo y seed idempotente.
- [x] 20 pruebas automatizadas superadas.
- [x] Documentación de ejecución, decisiones técnicas y uso de asistencia de IA.
