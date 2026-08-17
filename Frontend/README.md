# Quom Asset Management — Frontend

Frontend web de **Quom Asset Management**, orientado a la consulta y operación de activos de TI. La aplicación consume la API del proyecto, presenta flujos diferenciados por rol y ofrece una interfaz responsiva con estados claros de carga, vacío, error y confirmación.

Este documento describe exclusivamente la aplicación Blazor ubicada en la carpeta `Frontend`.

## Propósito

La aplicación permite a usuarios autenticados consultar la información operativa de activos, empleados y proveedores. Los usuarios con rol **Administrador** disponen además de las acciones de registro y administración habilitadas por la API.

La interfaz refleja reglas de negocio para prevenir operaciones inválidas y mejorar la experiencia, pero la API continúa siendo la autoridad para autenticación, autorización y validación de datos.

## Stack tecnológico

- .NET 8
- Blazor Web App
- Componentes Razor con interactividad del servidor (`InteractiveServer`)
- `HttpClient` y `System.Net.Http.Json` para integración con la API REST
- Autenticación mediante token JWT tipo Bearer
- Persistencia de sesión en el almacenamiento del navegador
- CSS personalizado y utilidades de Tailwind CSS
- Diseño responsivo para escritorio y pantallas de menor tamaño

## Requisitos

- .NET SDK 8 instalado.
- API de Quom Asset Management en ejecución.
- Una URL accesible para la API.
- Un navegador moderno con JavaScript y almacenamiento web habilitados.
- Credenciales válidas de un usuario Administrador u Operador.

## Configuración de la API

La dirección base se obtiene de `ApiSettings:BaseUrl`. Configúrala en el archivo de configuración del proyecto web, por ejemplo en `appsettings.json` o en el archivo correspondiente al ambiente:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001/"
  }
}
```

Consideraciones:

- La URL debe apuntar a la instancia real de la API.
- Debe conservar la diagonal final para resolver correctamente las rutas relativas.
- El esquema HTTP/HTTPS y el puerto deben coincidir con el perfil usado por la API.
- Si frontend y API se sirven desde orígenes distintos, la API debe autorizar el origen del frontend mediante su configuración CORS.
- No se debe dejar una dirección local fija para ambientes de prueba, integración o producción; use configuración específica por ambiente.

El valor se utiliza como `BaseAddress` de los clientes HTTP registrados para los servicios del frontend.

## Ejecución

Desde la carpeta que contiene el proyecto web:

```bash
dotnet restore
dotnet run
```

Después, abre en el navegador la URL mostrada por la aplicación. La API debe estar iniciada y disponible en la dirección definida por `ApiSettings:BaseUrl`.

Para desarrollo con recarga automática puede utilizarse:

```bash
dotnet watch run
```

## Autenticación y sesión

El inicio de sesión solicita usuario o correo electrónico y contraseña. El frontend envía las credenciales al endpoint de autenticación y, cuando la respuesta es válida, conserva en el estado de autenticación los datos necesarios de la sesión, incluido el JWT y el rol.

El flujo implementado incluye:

1. Autenticación contra la API.
2. Almacenamiento de la sesión en el navegador para conservarla al recargar la página.
3. Restauración del estado al iniciar la aplicación.
4. Envío del encabezado `Authorization: Bearer <token>` en las solicitudes protegidas.
5. Limpieza de la sesión al cerrar sesión.
6. Limpieza de la sesión cuando la API responde `401 Unauthorized`, mostrando que la sesión expiró.

La persistencia del cliente no extiende la vigencia del token ni sustituye la validación del servidor.

## Roles y permisos

### Administrador

Puede consultar los módulos y ejecutar las operaciones administrativas expuestas por la API e implementadas en la interfaz, entre ellas:

- Registrar activos.
- Registrar empleados.
- Registrar proveedores.
- Administrar los tipos de servicio asociados a un proveedor.
- Ejecutar los flujos operativos de asignación y devolución disponibles para los activos.

### Operador

Dispone de acceso de consulta a la información habilitada para usuarios autenticados. La interfaz oculta los botones administrativos y muestra mensajes de operación restringida si se intenta acceder directamente a una vista reservada.

La visibilidad condicional de controles es una medida de experiencia de usuario. La autorización efectiva debe aplicarse siempre en la API; ocultar un botón no constituye un control de seguridad suficiente.

## Estructura sugerida del frontend

```text
Vista/
├── Components/
│   ├── Layout/                # Layout principal y navegación
│   └── Pages/                 # Vistas Razor por módulo
│       ├── Login.razor
│       ├── Assets.razor
│       ├── AssetCreate.razor
│       ├── Employees.razor
│       ├── EmployeeCreate.razor
│       ├── Suppliers.razor
│       ├── SupplierCreate.razor
│       ├── SupplierServices.razor
│       └── SupplierServicesManage.razor
├── Models/                    # Modelos de lectura, solicitudes y respuestas
├── Services/                  # Clientes HTTP de autenticación y módulos
├── State/                     # Estado de autenticación y sesión
├── wwwroot/                   # Estilos y recursos estáticos
├── appsettings.json           # ApiSettings:BaseUrl
└── Program.cs                 # Registro de servicios y clientes HTTP
```

Los nombres pueden variar ligeramente en la solución final; la separación de responsabilidades es la relevante: páginas, modelos, acceso a API y estado de sesión.

## Vistas y funcionalidades

### Inicio de sesión

- Captura de usuario o correo y contraseña.
- Alternancia de visibilidad de contraseña.
- Bloqueo visual del formulario durante el envío.
- Mensajes de validación y errores devueltos por la API.
- Navegación a la aplicación después de autenticar correctamente.

### Activos

- Listado de activos con información operativa y estado.
- Búsqueda, filtros y paginación en la vista de consulta.
- Registro de activos para Administradores.
- Selección de datos relacionados requeridos por el tipo de activo.
- Consulta del detalle e historial del activo.
- Flujo de asignación de un activo a un empleado.
- Flujo de devolución de un activo previamente asignado.
- Actualización visual de los datos después de una operación satisfactoria.

No se presentan acciones genéricas de edición o eliminación cuando no existe un contrato equivalente en la API.

### Empleados

- Listado de empleados para usuarios autenticados.
- Búsqueda y paginación del listado.
- Resumen de empleados, departamentos y ubicaciones disponibles.
- Alta de empleado exclusiva para Administradores.
- Captura de número de empleado, nombre, apellidos y correo como datos obligatorios.
- Captura opcional de departamento y ubicación.

Los empleados registrados quedan disponibles para los flujos de asignación de activos. El frontend no ofrece edición ni eliminación porque esas operaciones no forman parte del contrato implementado.

### Proveedores

- Listado de proveedores para usuarios autenticados.
- Búsqueda y paginación.
- Visualización de nombre, RFC o Tax ID, contacto, correo, teléfono y estado.
- Alta de proveedor exclusiva para Administradores.
- Nombre obligatorio y datos fiscales y de contacto opcionales.
- Consulta de los tipos de servicio asociados a cada proveedor.
- Administración de asociaciones de servicios exclusiva para Administradores.

La consulta de servicios es de solo lectura para el Operador. La vista administrativa permite seleccionar y guardar la asociación vigente conforme al contrato de la API.

### Historial, asignaciones y devoluciones

El historial del activo presenta los movimientos registrados por el backend para conservar la trazabilidad operativa. Los flujos implementados distinguen entre:

- **Asignación:** relaciona el activo con un empleado mediante la operación específica de asignación.
- **Devolución:** cierra la asignación vigente mediante la operación específica de devolución.
- **Historial:** consulta los movimientos resultantes; no permite alterar registros históricos desde el frontend.

El frontend solicita confirmación y muestra estados de proceso y resultado cuando corresponde. La identidad del usuario que ejecuta el movimiento no se captura manualmente: debe obtenerse del JWT en el servidor.

## Reglas de negocio reflejadas en la interfaz

- El estado **Asignado** no se establece manualmente al registrar un activo; se obtiene mediante el flujo de asignación.
- La devolución se ofrece sobre un activo que cuenta con una asignación vigente.
- Los activos arrendados requieren proveedor y fecha de término del arrendamiento.
- Los campos dependientes del tipo de propiedad se muestran o exigen sólo cuando corresponden.
- Las operaciones administrativas sólo se muestran para el rol Administrador.
- El alta de empleado exige número de empleado, nombre, apellidos y correo; departamento y ubicación son opcionales.
- El alta de proveedor exige nombre; RFC o Tax ID, contacto, correo y teléfono son opcionales, con validación de formato para el correo cuando se proporciona.
- La actualización de servicios de un proveedor requiere seleccionar al menos un tipo de servicio.
- No se muestran acciones de edición o eliminación para módulos cuyos endpoints no fueron implementados.

Estas validaciones anticipan errores y orientan al usuario. Todas deben repetirse o confirmarse en la API y, cuando corresponda, en la base de datos.

## Manejo de errores y estados de interfaz

Las vistas contemplan:

- Indicadores de carga durante las solicitudes.
- Deshabilitación temporal de controles para evitar envíos duplicados.
- Estados vacíos cuando no existen registros o resultados de búsqueda.
- Mensajes de validación antes de enviar formularios.
- Mensajes devueltos por la API cuando pueden interpretarse de forma segura.
- Mensajes alternativos cuando la respuesta no contiene un detalle utilizable.
- Tratamiento diferenciado de `401 Unauthorized` y `403 Forbidden`.
- Limpieza de sesión ante expiración o invalidez del token.
- Acciones de reintento en consultas que no pudieron completarse.

Los errores técnicos internos no deben exponerse directamente al usuario final.

## Diseño visual

La aplicación utiliza una estética profesional oscura, con superficies tipo glass de baja intensidad. El diseño prioriza legibilidad y jerarquía sobre efectos decorativos:

- Fondo oscuro y paleta neutra basada en grises y tonos slate.
- Paneles semitransparentes con desenfoque y bordes discretos.
- Sombras, brillos y animaciones sutiles.
- Tablas, formularios, tarjetas de resumen y estados vacíos consistentes.
- Navegación y acciones adaptadas al rol autenticado.
- Distribución responsiva.
- Respeto a `prefers-reduced-motion` en efectos animados.

## Notas de seguridad

- No incluir credenciales, tokens, secretos ni URLs privadas en el repositorio.
- No registrar el JWT, contraseñas ni respuestas sensibles en consola o telemetría del navegador.
- Utilizar HTTPS fuera del desarrollo local.
- Considerar el almacenamiento del navegador accesible al código ejecutado en el mismo origen; una vulnerabilidad XSS podría exponer la sesión.
- Mantener una política de seguridad de contenido y evitar renderizar HTML no confiable.
- Validar expiración, firma, emisor y audiencia del JWT en la API.
- Aplicar autorización por rol en cada endpoint, independientemente de lo que muestre la interfaz.
- Configurar CORS con orígenes explícitos y evitar políticas abiertas en producción.
- Tratar toda entrada del cliente como no confiable.
- La identidad usada para auditoría y movimientos debe derivarse del JWT validado, no de un identificador enviado por la interfaz.

## Alcance

Este frontend implementa los flujos descritos para autenticación, activos, empleados, proveedores, servicios, historial, asignaciones y devoluciones. No debe asumirse soporte para editar o eliminar entidades, recuperación de contraseña, administración de usuarios u otras funciones no expuestas por las vistas y contratos documentados.
