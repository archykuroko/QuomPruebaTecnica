USE [Quom];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '========================================';
PRINT ' CARGA DE DATOS DEMO - QUOM';
PRINT '========================================';
PRINT '';

------------------------------------------------------------
-- 1. ROLES
------------------------------------------------------------
PRINT '1. Roles';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Administrador')
BEGIN
    INSERT INTO Roles (Name)
    VALUES ('Administrador');
END;

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Operador')
BEGIN
    INSERT INTO Roles (Name)
    VALUES ('Operador');
END;

DECLARE @AdminRoleId INT =
(
    SELECT Id
    FROM Roles
    WHERE Name = 'Administrador'
);

DECLARE @OperatorRoleId INT =
(
    SELECT Id
    FROM Roles
    WHERE Name = 'Operador'
);

------------------------------------------------------------
-- 2. USUARIOS DE PRUEBA
------------------------------------------------------------
PRINT '2. Usuarios';

-- IMPORTANTE:
-- El usuario admin ya debe existir en la BD restaurada.
-- Su contraseña de prueba es: Admin123!
--
-- El hash del operador corresponde a: Operador123!

DECLARE @OperatorHash VARCHAR(255) =
'AQAAAAIAAYagAAAAEBSBZQ0Txc0bVKOndiMDSj7lKgEOphUV/I5qz+J+GTVvY5CNT4AWi6pykcY4RdFn+w==';

IF NOT EXISTS (
    SELECT 1
    FROM Users
    WHERE Username = 'operador'
)
BEGIN
    INSERT INTO Users
    (
        Username,
        Email,
        PasswordHash,
        RoleId,
        IsActive,
        FailedLoginAttempts,
        LockoutEnd,
        CreatedAt
    )
    VALUES
    (
        'operador',
        'operador@empresa.com',
        @OperatorHash,
        @OperatorRoleId,
        1,
        0,
        NULL,
        SYSUTCDATETIME()
    );
END;

-- Normalizamos los usuarios demo por si se ejecuta el seed
-- después de realizar pruebas de bloqueo.
UPDATE Users
SET
    FailedLoginAttempts = 0,
    LockoutEnd = NULL,
    IsActive = 1
WHERE Username IN ('admin', 'operador');

------------------------------------------------------------
-- 3. TIPOS DE SERVICIO
------------------------------------------------------------
PRINT '3. Tipos de servicio';

IF NOT EXISTS (SELECT 1 FROM ServiceTypes WHERE Name = 'Compra')
BEGIN
    INSERT INTO ServiceTypes (Name)
    VALUES ('Compra');
END;

IF NOT EXISTS (SELECT 1 FROM ServiceTypes WHERE Name = 'Mantenimiento')
BEGIN
    INSERT INTO ServiceTypes (Name)
    VALUES ('Mantenimiento');
END;

IF NOT EXISTS (SELECT 1 FROM ServiceTypes WHERE Name = 'Arrendamiento')
BEGIN
    INSERT INTO ServiceTypes (Name)
    VALUES ('Arrendamiento');
END;

DECLARE @CompraId INT =
(
    SELECT Id FROM ServiceTypes WHERE Name = 'Compra'
);

DECLARE @MantenimientoId INT =
(
    SELECT Id FROM ServiceTypes WHERE Name = 'Mantenimiento'
);

DECLARE @ArrendamientoId INT =
(
    SELECT Id FROM ServiceTypes WHERE Name = 'Arrendamiento'
);

------------------------------------------------------------
-- 4. COLABORADORES
------------------------------------------------------------
PRINT '4. Colaboradores';

IF NOT EXISTS (
    SELECT 1 FROM Employees WHERE EmployeeNumber = 'EMP-001'
)
BEGIN
    EXEC usp_Employees_Create
        @EmployeeNumber = 'EMP-001',
        @FirstName = 'Ana',
        @LastName = 'Martínez López',
        @Email = 'ana.martinez@empresa.com',
        @Department = 'Tecnologías de Información',
        @Location = 'CDMX';
END;

IF NOT EXISTS (
    SELECT 1 FROM Employees WHERE EmployeeNumber = 'EMP-002'
)
BEGIN
    EXEC usp_Employees_Create
        @EmployeeNumber = 'EMP-002',
        @FirstName = 'Carlos',
        @LastName = 'Hernández Ruiz',
        @Email = 'carlos.hernandez@empresa.com',
        @Department = 'Finanzas',
        @Location = 'CDMX';
END;

IF NOT EXISTS (
    SELECT 1 FROM Employees WHERE EmployeeNumber = 'EMP-003'
)
BEGIN
    EXEC usp_Employees_Create
        @EmployeeNumber = 'EMP-003',
        @FirstName = 'Mariana',
        @LastName = 'Torres Sánchez',
        @Email = 'mariana.torres@empresa.com',
        @Department = 'Recursos Humanos',
        @Location = 'Monterrey';
END;

IF NOT EXISTS (
    SELECT 1 FROM Employees WHERE EmployeeNumber = 'EMP-004'
)
BEGIN
    EXEC usp_Employees_Create
        @EmployeeNumber = 'EMP-004',
        @FirstName = 'Diego',
        @LastName = 'Ramírez Castillo',
        @Email = 'diego.ramirez@empresa.com',
        @Department = 'Operaciones',
        @Location = 'Guadalajara';
END;

------------------------------------------------------------
-- 5. PROVEEDORES
------------------------------------------------------------
PRINT '5. Proveedores';

IF NOT EXISTS (
    SELECT 1 FROM Suppliers WHERE TaxId = 'DEM010101AA1'
)
BEGIN
    EXEC usp_Suppliers_Create
        @Name = 'Dell México',
        @TaxId = 'DEM010101AA1',
        @ContactName = 'Laura Gómez',
        @Email = 'contacto.dell@proveedor.com',
        @Phone = '5551001001';
END;

IF NOT EXISTS (
    SELECT 1 FROM Suppliers WHERE TaxId = 'TEM020202BB2'
)
BEGIN
    EXEC usp_Suppliers_Create
        @Name = 'Telcel Empresarial',
        @TaxId = 'TEM020202BB2',
        @ContactName = 'Ricardo Vargas',
        @Email = 'empresas.telcel@proveedor.com',
        @Phone = '5551001002';
END;

IF NOT EXISTS (
    SELECT 1 FROM Suppliers WHERE TaxId = 'TRS030303CC3'
)
BEGIN
    EXEC usp_Suppliers_Create
        @Name = 'TechRent Solutions',
        @TaxId = 'TRS030303CC3',
        @ContactName = 'Fernanda Silva',
        @Email = 'ventas@techrent.com',
        @Phone = '5551001003';
END;

DECLARE @DellSupplierId INT =
(
    SELECT Id
    FROM Suppliers
    WHERE TaxId = 'DEM010101AA1'
);

DECLARE @TelcelSupplierId INT =
(
    SELECT Id
    FROM Suppliers
    WHERE TaxId = 'TEM020202BB2'
);

DECLARE @TechRentSupplierId INT =
(
    SELECT Id
    FROM Suppliers
    WHERE TaxId = 'TRS030303CC3'
);

------------------------------------------------------------
-- 6. SERVICIOS DE PROVEEDORES
------------------------------------------------------------
PRINT '6. Servicios de proveedores';

DECLARE @DellServices NVARCHAR(MAX);
DECLARE @TelcelServices NVARCHAR(MAX);
DECLARE @TechRentServices NVARCHAR(MAX);

SET @DellServices =
    CONCAT('[', @CompraId, ',', @MantenimientoId, ']');

SET @TelcelServices =
    CONCAT('[', @CompraId, ',', @ArrendamientoId, ']'); 

SET @TechRentServices =
    CONCAT('[', @MantenimientoId, ',', @ArrendamientoId, ']');

EXEC usp_Suppliers_SetServices
    @SupplierId = @DellSupplierId,
    @ServiceTypeIds = @DellServices;

EXEC usp_Suppliers_SetServices
    @SupplierId = @TelcelSupplierId,
    @ServiceTypeIds = @TelcelServices;

EXEC usp_Suppliers_SetServices
    @SupplierId = @TechRentSupplierId,
    @ServiceTypeIds = @TechRentServices;
------------------------------------------------------------
-- 7. USUARIO QUE EJECUTA LA CARGA
------------------------------------------------------------
DECLARE @AdminUserId INT =
(
    SELECT Id
    FROM Users
    WHERE Username = 'admin'
);

IF @AdminUserId IS NULL
BEGIN
    THROW 50900,
        'El usuario admin es requerido para cargar los activos demo.',
        1;
END;

------------------------------------------------------------
-- 8. ACTIVOS
------------------------------------------------------------
PRINT '7. Activos';

------------------------------------------------------------
-- Laptop Dell - Disponible
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-001'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-001',
        @SerialNumber = 'DEMO-DELL-001',
        @Category = 'Laptop',
        @Brand = 'Dell',
        @Model = 'Latitude 5440',
        @OwnershipType = 'Propio',
        @SupplierId = @DellSupplierId,
        @Status = 'Disponible',
        @CurrentLocation = 'Almacén TI',
        @PurchaseDate = '2026-01-15',
        @RentalEndDate = NULL,
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- Laptop Lenovo - se asignará posteriormente
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-002'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-002',
        @SerialNumber = 'DEMO-LENOVO-002',
        @Category = 'Laptop',
        @Brand = 'Lenovo',
        @Model = 'ThinkPad T14',
        @OwnershipType = 'Propio',
        @SupplierId = @DellSupplierId,
        @Status = 'Disponible',
        @CurrentLocation = 'Oficina Central',
        @PurchaseDate = '2026-02-10',
        @RentalEndDate = NULL,
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- HP en mantenimiento
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-003'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-003',
        @SerialNumber = 'DEMO-HP-003',
        @Category = 'Laptop',
        @Brand = 'HP',
        @Model = 'EliteBook 840 G10',
        @OwnershipType = 'Propio',
        @SupplierId = @DellSupplierId,
        @Status = 'Mantenimiento',
        @CurrentLocation = 'Taller de Soporte',
        @PurchaseDate = '2025-11-20',
        @RentalEndDate = NULL,
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- iPhone arrendado
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-004'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-004',
        @SerialNumber = 'DEMO-IP15-004',
        @Category = 'Celular',
        @Brand = 'Apple',
        @Model = 'iPhone 15',
        @OwnershipType = 'Arrendado',
        @SupplierId = @TelcelSupplierId,
        @Status = 'Disponible',
        @CurrentLocation = 'Almacén TI',
        @PurchaseDate = NULL,
        @RentalEndDate = '2027-08-01',
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- Monitor
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-005'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-005',
        @SerialNumber = 'DEMO-DELL-MON-005',
        @Category = 'Monitor',
        @Brand = 'Dell',
        @Model = 'P2422H',
        @OwnershipType = 'Propio',
        @SupplierId = @DellSupplierId,
        @Status = 'Disponible',
        @CurrentLocation = 'Almacén TI',
        @PurchaseDate = '2025-09-12',
        @RentalEndDate = NULL,
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- Impresora arrendada
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-006'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-006',
        @SerialNumber = 'DEMO-PRINTER-006',
        @Category = 'Impresora',
        @Brand = 'HP',
        @Model = 'LaserJet Pro',
        @OwnershipType = 'Arrendado',
        @SupplierId = @TechRentSupplierId,
        @Status = 'Disponible',
        @CurrentLocation = 'Finanzas',
        @PurchaseDate = NULL,
        @RentalEndDate = '2027-02-28',
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- Periférico
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-007'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-007',
        @SerialNumber = 'DEMO-LOGI-007',
        @Category = 'Periférico',
        @Brand = 'Logitech',
        @Model = 'MX Keys',
        @OwnershipType = 'Propio',
        @SupplierId = NULL,
        @Status = 'Disponible',
        @CurrentLocation = 'Almacén TI',
        @PurchaseDate = '2026-03-05',
        @RentalEndDate = NULL,
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- Activo retirado
------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM Assets WHERE AssetCode = 'DEMO-TI-008'
)
BEGIN
    EXEC usp_Assets_Create
        @AssetCode = 'DEMO-TI-008',
        @SerialNumber = 'DEMO-DELL-OLD-008',
        @Category = 'Laptop',
        @Brand = 'Dell',
        @Model = 'Latitude 5490',
        @OwnershipType = 'Propio',
        @SupplierId = @DellSupplierId,
        @Status = 'Retirado',
        @CurrentLocation = 'Bodega de Baja',
        @PurchaseDate = '2019-06-10',
        @RentalEndDate = NULL,
        @PerformedByUserId = @AdminUserId;
END;

------------------------------------------------------------
-- 9. ASIGNACIÓN DEMO
------------------------------------------------------------
PRINT '8. Asignaciones';

DECLARE @AssignedAssetId INT =
(
    SELECT Id
    FROM Assets
    WHERE AssetCode = 'DEMO-TI-002'
);

DECLARE @EmployeeId INT =
(
    SELECT Id
    FROM Employees
    WHERE EmployeeNumber = 'EMP-001'
);

-- Solamente se asigna si continúa disponible.
IF EXISTS (
    SELECT 1
    FROM Assets
    WHERE Id = @AssignedAssetId
      AND Status = 'Disponible'
)
BEGIN
    EXEC usp_Assets_Assign
        @AssetId = @AssignedAssetId,
        @EmployeeId = @EmployeeId,
        @PerformedByUserId = @AdminUserId,
        @Notes = 'Asignación inicial incluida en los datos de demostración.';
END;

------------------------------------------------------------
-- 10. RESULTADO
------------------------------------------------------------
PRINT '';
PRINT '========================================';
PRINT ' DATOS DEMO CARGADOS CORRECTAMENTE';
PRINT '========================================';

SELECT
    'Roles' AS Catalog,
    COUNT(*) AS Total
FROM Roles

UNION ALL

SELECT
    'Users',
    COUNT(*)
FROM Users

UNION ALL

SELECT
    'Employees',
    COUNT(*)
FROM Employees

UNION ALL

SELECT
    'Suppliers',
    COUNT(*)
FROM Suppliers

UNION ALL

SELECT
    'Assets',
    COUNT(*)
FROM Assets;

GO