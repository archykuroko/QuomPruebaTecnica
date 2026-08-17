# Quom Asset Management

**Prueba técnica desarrollada por Steven Escárcega.**

## Objetivo

Solución para administrar activos de TI, empleados, proveedores, servicios y el ciclo de asignación y devolución de activos, manteniendo control de acceso y trazabilidad de las operaciones.

## Resumen de la solución

| Módulo | Implementación |
|---|---|
| Base de datos | SQL Server con operaciones mediante Stored Procedures |
| API / Backend | ASP.NET Core Web API sobre .NET 8, organizada con Clean Architecture |
| Frontend | Blazor Interactive Server |
| Seguridad | Autenticación JWT y autorización por roles: Administrador y Operador |

## Funcionalidades implementadas

- Administración de activos, empleados, proveedores y servicios.
- Asignación y devolución de activos.
- Consulta de historial y trazabilidad de movimientos.
- Autenticación y autorización según el rol del usuario.
- Bloqueo temporal de acceso (*lockout*) ante intentos fallidos de autenticación.
- Validación de reglas de negocio durante las operaciones críticas.

## Estructura del repositorio

La solución está separada por módulos para facilitar su revisión y mantenimiento:

```text
Quom-Asset-Management/
├── BD/          # Base de datos, scripts y Stored Procedures
├── Backend/         # API REST, lógica de negocio y seguridad
└── Frontend/    # Aplicación web en Blazor
```

Cada carpeta contiene su propio `README.md` con instrucciones, arquitectura, configuración y detalles de implementación específicos. Este documento funciona únicamente como vista general de la entrega.

## Decisiones y justificaciones técnicas

- **Stored Procedures:** se utilizaron para las operaciones de base de datos en cumplimiento de los requisitos de la prueba.
- **Clean Architecture:** permite separar dominio, aplicación, infraestructura y presentación, reduciendo el acoplamiento entre responsabilidades.
- **JWT y roles:** proporcionan autenticación sin estado y autorización diferenciada para Administrador y Operador.
- **Identidad desde el token:** la identidad responsable de cada operación se obtiene del JWT validado, evitando confiar en identificadores enviados por el cliente.
- **Reglas críticas en servidor:** las validaciones determinantes se mantienen en la API y la base de datos. La interfaz las anticipa cuando es conveniente para mejorar la experiencia, sin sustituir la validación del servidor.
- **Blazor Interactive Server:** mantiene el ecosistema .NET de extremo a extremo y permite construir una interfaz interactiva integrada con la API.

## Uso de inteligencia artificial

Se utilizaron herramientas de inteligencia artificial como apoyo durante el desarrollo, con el siguiente alcance:

| Módulo | Uso de IA |
|---|---|
| Base de datos | Revisión de Stored Procedures, análisis de casos y apoyo en documentación |
| API / Backend | Depuración, refactorización y revisión de validaciones |
| Frontend | Apoyo en estructura, estilos y depuración de la interfaz |
| Documentación | Organización, redacción y revisión de claridad y consistencia |

Las sugerencias generadas fueron revisadas, adaptadas al contexto de la solución y probadas manualmente. Las decisiones técnicas, la integración de los módulos y la validación final permanecieron bajo responsabilidad del autor.

## Tiempo de desarrollo

El desarrollo requirió **aproximadamente 11 horas efectivas**, distribuidas entre el diseño de la base de datos, la implementación e integración de la API y el frontend, las pruebas funcionales y la documentación. No se presenta un desglose exacto por módulo debido a que el trabajo se realizó de manera incremental e intercalada.

## Ejecución

Los requisitos, pasos de configuración y comandos de ejecución se encuentran en los `README.md` de las carpetas **BD**, **API** y **Frontend**. Se recomienda prepararlos y ejecutarlos en ese orden.

## Cierre

La entrega prioriza separación de responsabilidades, seguridad, trazabilidad y claridad de mantenimiento, manteniendo documentado cada módulo de forma independiente.
