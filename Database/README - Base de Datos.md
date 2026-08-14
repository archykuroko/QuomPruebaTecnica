# Sistema de Gestión y Resguardo de Activos TI

## Base de Datos

Este directorio contiene los scripts y la documentación correspondiente a la capa de datos del Sistema de Gestión y Resguardo de Activos TI.

La base de datos fue desarrollada utilizando **Microsoft SQL Server** y está diseñada para soportar la administración de activos tecnológicos, colaboradores, proveedores, usuarios del sistema, asignaciones, devoluciones e historial de movimientos.

La solución utiliza **Stored Procedures como mecanismo principal de acceso y modificación de datos**, ya que la aplicación utilizará ADO.NET mediante `Microsoft.Data.SqlClient` y no Entity Framework Core.

---

## 1. Objetivos del diseño

El modelo de datos fue diseñado considerando los siguientes objetivos:

- Mantener la integridad de la información mediante Primary Keys, Foreign Keys, restricciones e índices únicos.
- Mantener trazabilidad de las operaciones realizadas sobre los activos.
- Conservar el historial de asignaciones y devoluciones.
- Evitar que un activo tenga más de una asignación activa simultáneamente.
- Permitir el control de concurrencia durante operaciones críticas.
- Mantener separadas las cuentas que utilizan el sistema de los colaboradores que reciben activos.
- Proporcionar operaciones de acceso a datos mediante Stored Procedures.
- Preparar la información necesaria para autenticación, autorización y bloqueo temporal de usuarios.

---

## 2. Tablas

La base de datos está compuesta actualmente por las siguientes tablas:

### Roles

Contiene los perfiles disponibles para los usuarios del sistema.

Roles iniciales:

- Administrador
- Operador

### Users

Representa las cuentas que pueden autenticarse en el sistema.

Entre sus datos se encuentran:

- Username
- Email
- PasswordHash
- RoleId
- IsActive
- FailedLoginAttempts
- LockoutEnd
- CreatedAt
- UpdatedAt

Las contraseñas no se almacenan en texto plano. La generación y validación del hash se realizará desde la aplicación .NET.

### Employees

Representa a los colaboradores de la empresa que pueden recibir activos.

Un colaborador no necesariamente corresponde a un usuario del sistema.

Contiene información como:

- EmployeeNumber
- FirstName
- LastName
- Email
- Department
- Location
- IsActive
- CreatedAt
- UpdatedAt

El número de empleado debe ser único.

### Suppliers

Almacena los proveedores relacionados con los activos y servicios de TI.

Incluye:

- Name
- TaxId
- ContactName
- Email
- Phone
- IsActive
- CreatedAt
- UpdatedAt

### ServiceTypes

Catálogo de servicios que puede proporcionar un proveedor.

Los valores iniciales considerados son:

- Compra
- Mantenimiento
- Arrendamiento

### SupplierServices

Tabla de relación entre proveedores y tipos de servicio.

Permite que un mismo proveedor pueda proporcionar más de un servicio.

### Assets

Tabla principal para el almacenamiento de los activos tecnológicos.

Contiene:

- Id
- AssetCode
- SerialNumber
- Category
- Brand
- Model
- OwnershipType
- SupplierId
- Status
- CurrentLocation
- PurchaseDate
- RentalEndDate
- CreatedAt
- UpdatedAt

Estados soportados:

- Disponible
- Asignado
- Mantenimiento
- Retirado

Tipos de propiedad soportados:

- Propio
- Arrendado

Un activo arrendado requiere un proveedor asociado.

Adicionalmente, se estableció como regla de integridad que un activo arrendado cuente con fecha de término de arrendamiento.

### AssetAssignments

Mantiene el historial de asignaciones de activos a colaboradores.

Una asignación permanece activa mientras `ReturnedAt` sea `NULL`.

Incluye:

- AssetId
- EmployeeId
- AssignedByUserId
- AssignedAt
- ReturnedAt
- ReturnedByUserId
- ReturnCondition
- Notes

La devolución no elimina el registro de asignación. En su lugar, se registra la fecha de devolución y el usuario que realizó la operación, conservando así la trazabilidad.

### AssetMovements

Funciona como bitácora de auditoría de los activos.

Permite registrar eventos como:

- Assigned
- Returned
- AssetCreated
- StatusChanged
- LocationChanged

Puede almacenar:

- Estado anterior y nuevo.
- Ubicación anterior y nueva.
- Usuario que realizó la acción.
- Fecha y hora.
- Observaciones.

---

## 3. Relaciones principales

Las principales relaciones del modelo son:

```text
Roles
  |
  +---- Users

Suppliers
  |
  +---- Assets
  |
  +---- SupplierServices ---- ServiceTypes

Employees
  |
  +---- AssetAssignments ---- Assets
                |
                +---- Users (AssignedByUserId)
                |
                +---- Users (ReturnedByUserId)

Assets
  |
  +---- AssetMovements ---- Users
```

Las relaciones se encuentran protegidas mediante Foreign Keys.

No se utiliza eliminación en cascada para información histórica. La intención es evitar que la eliminación de un usuario, colaborador o entidad relacionada provoque la pérdida accidental de información de auditoría.

---

## 4. Índices

### AssetCode

Se creó un índice único:

```sql
UX_Assets_AssetCode
```

Este índice garantiza que no puedan existir dos activos con el mismo código y permite búsquedas eficientes por `AssetCode`.

### SerialNumber

Se creó el índice:

```sql
UX_Assets_SerialNumber
```

Es un índice:

```text
Unique
Non-Clustered
Filtered
```

El filtro permite excluir registros cuyo `SerialNumber` sea `NULL`.

De esta manera pueden existir activos sin número de serie, pero cuando éste se proporciona debe ser único.

### Asignaciones activas

Se creó:

```sql
UX_AssetAssignments_ActiveAsset
```

con la siguiente lógica:

```sql
CREATE UNIQUE INDEX UX_AssetAssignments_ActiveAsset
ON AssetAssignments(AssetId)
WHERE ReturnedAt IS NULL;
```

Este índice representa una protección adicional de integridad para evitar que un activo pueda tener dos asignaciones activas simultáneamente.

Los registros históricos permanecen permitidos porque una asignación devuelta deja de cumplir la condición `ReturnedAt IS NULL`.

### Usuarios

Se utilizan índices únicos para:

```text
UX_Users_Username
UX_Users_Email
```

Esto evita duplicados de nombres de usuario y correos electrónicos incluso si existiera un error en las validaciones de la aplicación o del Stored Procedure.

---

## 5. Constraints

Se utilizaron restricciones a nivel de SQL Server para proteger reglas fundamentales del dominio.

Entre ellas se encuentran:

### Estado del activo

Sólo se permiten:

```text
Disponible
Asignado
Mantenimiento
Retirado
```

### Tipo de propiedad

Sólo se permiten:

```text
Propio
Arrendado
```

### Activos arrendados

Un activo de tipo `Arrendado` debe tener proveedor asociado.

Como regla adicional del diseño, también debe contar con una fecha de término de arrendamiento.

### Fechas de asignación

Una devolución no puede tener una fecha anterior a la fecha en que el activo fue asignado.

### Usuario que registra una devolución

Cuando una asignación contiene fecha de devolución, debe existir también el usuario responsable de registrar dicha devolución.

---

## 6. Stored Procedures

La solución utiliza Stored Procedures para las operaciones principales.

### Colaboradores

```text
usp_Employees_Create
usp_Employees_GetAll
```

Permiten registrar y consultar colaboradores.

### Proveedores

```text
usp_Suppliers_Create
usp_Suppliers_GetAll
```

Permiten registrar y consultar proveedores.

### Activos

```text
usp_Assets_Create
usp_Assets_GetById
usp_Assets_Search
usp_Assets_Update
usp_Assets_GetHistory
```

`usp_Assets_Search` soporta:

- Búsqueda.
- Filtro por estado.
- Filtro por categoría.
- Paginación.

### Asignaciones

```text
usp_Assets_Assign
```

Realiza de manera transaccional las siguientes operaciones:

1. Valida al usuario que realiza la operación.
2. Valida que el activo exista.
3. Obtiene un bloqueo sobre el activo.
4. Valida que el activo se encuentre disponible.
5. Valida que el colaborador exista.
6. Valida que el colaborador esté activo.
7. Verifica que no exista otra asignación activa.
8. Crea la asignación.
9. Cambia el estado del activo a `Asignado`.
10. Registra el movimiento de auditoría.
11. Confirma la transacción.

Si alguna operación falla, se realiza `ROLLBACK`.

### Devoluciones

```text
usp_Assets_Return
```

Realiza:

1. Validación del usuario.
2. Validación y bloqueo del activo.
3. Validación del estado del activo.
4. Búsqueda de la asignación activa.
5. Cierre de la asignación.
6. Registro de fecha y condición de devolución.
7. Cambio del activo a `Disponible`.
8. Registro del movimiento de auditoría.
9. Confirmación de la transacción.

---

## 7. Manejo de concurrencia

La asignación de activos fue identificada como una operación crítica debido a la posibilidad de recibir dos solicitudes simultáneas para el mismo activo.

Para reducir este riesgo se implementaron diferentes niveles de protección.

### Transacción

La asignación se realiza dentro de una transacción.

Esto garantiza que la creación de la asignación, actualización del activo y registro del movimiento se confirmen como una sola unidad de trabajo.

Si una operación falla, los cambios realizados durante la transacción son revertidos.

### UPDLOCK y HOLDLOCK

Durante la asignación se consulta el activo utilizando:

```sql
WITH (UPDLOCK, HOLDLOCK)
```

El bloqueo se conserva durante la transacción, evitando que otra transacción pueda realizar de forma concurrente una asignación incompatible sobre el mismo activo.

Cuando la segunda operación puede continuar, vuelve a observar el estado actualizado del activo y la validación de disponibilidad impide completar una segunda asignación.

### Índice único filtrado

Como protección adicional se utiliza:

```sql
UX_AssetAssignments_ActiveAsset
```

Este índice garantiza a nivel de base de datos que solamente pueda existir una asignación activa por activo.

La estrategia combina:

```text
Validación de negocio
        +
Transacción
        +
Bloqueo
        +
Índice único filtrado
```

De esta manera, la integridad de la información no depende únicamente de una validación realizada por la aplicación.

---

## 8. Pruebas realizadas

Los Stored Procedures principales fueron probados directamente en SQL Server antes de integrar la base de datos con la API.

### Alta de información

Se verificó correctamente:

- Alta de colaborador.
- Alta de proveedor.
- Alta de activo.
- Consulta de activo.
- Actualización de activo.

### Asignación

Se verificó:

- Asignación de un activo disponible.
- Cambio automático de `Disponible` a `Asignado`.
- Creación del registro en `AssetAssignments`.
- Creación del movimiento en `AssetMovements`.
- Registro del usuario responsable.
- Rechazo de una segunda asignación cuando el activo ya se encuentra asignado.

### Concurrencia

Se realizó una prueba manual utilizando dos sesiones independientes de SQL Server.

La primera sesión obtuvo un bloqueo sobre un activo disponible y mantuvo abierta la transacción temporalmente.

Durante ese periodo, una segunda sesión intentó obtener el mismo activo.

Se verificó que:

1. La segunda sesión permaneció esperando la liberación del bloqueo.
2. La primera sesión cambió el activo a `Asignado`.
3. La primera sesión confirmó la transacción.
4. La segunda sesión continuó posteriormente.
5. La segunda sesión obtuvo el estado actualizado `Asignado`.

Esta prueba permitió verificar experimentalmente el comportamiento esperado del mecanismo de concurrencia.

### Devolución

Se verificó:

- Cierre de la asignación activa.
- Registro de `ReturnedAt`.
- Registro de `ReturnedByUserId`.
- Registro de condición de devolución.
- Cambio del activo a `Disponible`.
- Creación del movimiento `Returned`.
- Conservación del movimiento `Assigned`.
- Rechazo de una segunda devolución sobre un activo ya disponible.

---

## 9. Autenticación y bloqueo de usuarios

La tabla `Users` contiene los campos necesarios para soportar autenticación y una política de bloqueo temporal.

Stored Procedures:

```text
usp_Auth_GetUserByUsername
usp_Auth_RegisterFailedAttempt
usp_Auth_ResetFailedAttempts
usp_Users_Create
```

La política inicial establecida es:

```text
Máximo de intentos fallidos: 5
Duración del bloqueo: 15 minutos
```

`usp_Auth_RegisterFailedAttempt` incrementa el contador de intentos y establece `LockoutEnd` cuando se alcanza el máximo permitido.

`usp_Auth_ResetFailedAttempts` restablece el contador después de una autenticación correcta.

La comparación de contraseñas y generación del JWT no se realizan dentro de SQL Server. Estas responsabilidades pertenecen a la capa de aplicación.

---

## 10. Seguridad

Se consideraron las siguientes medidas desde la capa de datos:

- No almacenar contraseñas en texto plano.
- Uso de parámetros en Stored Procedures.
- Foreign Keys para integridad referencial.
- Constraints para reglas fundamentales.
- Índices únicos para datos que no admiten duplicados.
- Transacciones para operaciones compuestas.
- Manejo controlado de errores mediante `THROW`.
- Protección contra asignaciones concurrentes.
- Conservación de información histórica.

Los detalles internos de SQL Server no deberán exponerse directamente al consumidor de la API. La traducción de excepciones SQL a respuestas controladas será responsabilidad del backend.

---

## 11. Uso de Inteligencia Artificial

Durante el desarrollo de la capa de datos se utilizó **ChatGPT de OpenAI** como herramienta de apoyo.

Su uso se concentró principalmente en:

- Revisión del diseño inicial del modelo relacional.
- Propuesta y revisión de restricciones e índices.
- Apoyo en la estructura de Stored Procedures.
- Análisis de alternativas para el manejo de concurrencia.
- Revisión de la estrategia basada en `UPDLOCK`, `HOLDLOCK` e índice único filtrado.
- Generación de casos de prueba para validar asignaciones y devoluciones.
- Apoyo en la elaboración de esta documentación.

El código y las propuestas generadas con apoyo de IA no fueron incorporados sin validación.

Las validaciones realizadas incluyeron:

- Ejecución manual de Stored Procedures en SQL Server.
- Verificación de registros generados en las tablas involucradas.
- Pruebas de reglas de negocio.
- Prueba de asignación duplicada.
- Prueba de devolución duplicada.
- Prueba manual de concurrencia utilizando dos sesiones independientes.
- Verificación de los estados antes y después de cada operación.
- Revisión de índices, Foreign Keys y Constraints directamente en SQL Server.

Las decisiones técnicas finales fueron tomadas considerando los requerimientos de la prueba y el comportamiento observado durante las pruebas.

Particularmente, se decidió utilizar una combinación de transacciones, bloqueos e índice único filtrado para proteger la asignación concurrente de activos, en lugar de depender exclusivamente de validaciones realizadas desde la aplicación.

---

## 12. Integración con la aplicación

La API se integrará con SQL Server mediante:

```text
ASP.NET Core Web API
        |
        v
ADO.NET / Microsoft.Data.SqlClient
        |
        v
Stored Procedures
        |
        v
SQL Server
```

No se utilizará Entity Framework Core para el acceso a datos.

La API será responsable de:

- Validaciones de entrada.
- Autenticación.
- Verificación segura de contraseñas.
- Generación y validación de JWT.
- Autorización mediante roles.
- Manejo global de excepciones.
- Traducción de errores de negocio a respuestas HTTP controladas.
- Consumo de los Stored Procedures mediante parámetros.

---

## 13. Estado actual

La capa de base de datos cuenta actualmente con:

- Modelo relacional.
- Primary Keys.
- Foreign Keys.
- Constraints.
- Índices únicos.
- Índice único filtrado para asignaciones activas.
- Stored Procedures de colaboradores.
- Stored Procedures de proveedores.
- Stored Procedures de activos.
- Asignación transaccional.
- Devolución transaccional.
- Historial de movimientos.
- Soporte de paginación y filtros.
- Stored Procedures necesarios para autenticación.
- Política inicial de intentos fallidos.
- Pruebas manuales de operaciones principales.
- Prueba manual de concurrencia.

La siguiente etapa del desarrollo consiste en integrar esta capa mediante ADO.NET con la API desarrollada en ASP.NET Core.