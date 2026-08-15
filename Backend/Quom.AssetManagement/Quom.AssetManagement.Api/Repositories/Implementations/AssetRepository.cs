using Microsoft.Data.SqlClient;
using Quom.AssetManagement.Api.Data;
using Quom.AssetManagement.Api.DTOs;
using Quom.AssetManagement.Api.DTOs.Assets;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using System.Data;

namespace Quom.AssetManagement.Api.Repositories.Implementations
{
    public class AssetRepository : IAssetRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public AssetRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Obtener activos por ID
        public async Task<Asset?> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "usp_Assets_GetById",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            // Se usan parámetros tipados para evitar concatenar valores
            // y mantener control sobre el tipo enviado a SQL Server
            command.Parameters.Add(
                new SqlParameter("@Id", SqlDbType.Int)
                {
                    Value = id
                });

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Asset
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                AssetCode = reader.GetString(reader.GetOrdinal("AssetCode")),
                SerialNumber = GetNullableString(reader, "SerialNumber"),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Brand = reader.GetString(reader.GetOrdinal("Brand")),
                Model = GetNullableString(reader, "Model"),
                OwnershipType = reader.GetString(reader.GetOrdinal("OwnershipType")),
                SupplierId = GetNullableInt(reader, "SupplierId"),
                SupplierName = GetNullableString(reader, "SupplierName"),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CurrentLocation = GetNullableString(reader, "CurrentLocation"),
                PurchaseDate = GetNullableDateTime(reader, "PurchaseDate"),
                RentalEndDate = GetNullableDateTime(reader, "RentalEndDate"),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        // Búsqueda de activos
        public async Task<PagedResult<Asset>> SearchAsync(AssetSearchRequest request)
        {
            var assets = new List<Asset>();
            var totalRecords = 0;

            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand("usp_Assets_Search", connection);

            command.CommandType = CommandType.StoredProcedure;

            // DBNull permite enviar filtros opcionales como NULL al Stored Procedure
            command.Parameters.Add("@Search", SqlDbType.VarChar, 100)
                .Value = (object?)request.Search ?? DBNull.Value;

            command.Parameters.Add("@Status", SqlDbType.VarChar, 20)
                .Value = (object?)request.Status ?? DBNull.Value;

            command.Parameters.Add("@Category", SqlDbType.VarChar, 50)
                .Value = (object?)request.Category ?? DBNull.Value;

            command.Parameters.Add("@PageNumber", SqlDbType.Int)
                .Value = request.PageNumber;

            command.Parameters.Add("@PageSize", SqlDbType.Int)
                .Value = request.PageSize;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                assets.Add(new Asset
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    AssetCode = reader.GetString(reader.GetOrdinal("AssetCode")),
                    SerialNumber = GetNullableString(reader, "SerialNumber"),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    Brand = reader.GetString(reader.GetOrdinal("Brand")),
                    Model = GetNullableString(reader, "Model"),
                    OwnershipType = reader.GetString(reader.GetOrdinal("OwnershipType")),
                    SupplierId = GetNullableInt(reader, "SupplierId"),
                    SupplierName = GetNullableString(reader, "SupplierName"),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    CurrentLocation = GetNullableString(reader, "CurrentLocation"),
                    PurchaseDate = GetNullableDateTime(reader, "PurchaseDate"),
                    RentalEndDate = GetNullableDateTime(reader, "RentalEndDate"),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
                });
            }

            // El SP devuelve el total en un segundo result set
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                totalRecords = reader.GetInt32(reader.GetOrdinal("TotalRecords"));
            }

            return new PagedResult<Asset>
            {
                Items = assets,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        // Creación de activos
        public async Task<int> CreateAsync(CreateAssetRequest request, int performedByUserId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand("usp_Assets_Create", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AssetCode", SqlDbType.VarChar, 50)
                .Value = request.AssetCode;

            command.Parameters.Add("@SerialNumber", SqlDbType.VarChar, 100)
                .Value = (object?)request.SerialNumber ?? DBNull.Value;

            command.Parameters.Add("@Category", SqlDbType.VarChar, 50)
                .Value = request.Category;

            command.Parameters.Add("@Brand", SqlDbType.VarChar, 100)
                .Value = request.Brand;

            command.Parameters.Add("@Model", SqlDbType.VarChar, 100)
                .Value = (object?)request.Model ?? DBNull.Value;

            command.Parameters.Add("@OwnershipType", SqlDbType.VarChar, 20)
                .Value = request.OwnershipType;

            command.Parameters.Add("@SupplierId", SqlDbType.Int)
                .Value = (object?)request.SupplierId ?? DBNull.Value;

            command.Parameters.Add("@PerformedByUserId", SqlDbType.Int)
                  .Value = performedByUserId;

            // Todo activo nuevo inicia Disponible
            command.Parameters.Add("@Status", SqlDbType.VarChar, 20)
                .Value = "Disponible";

            command.Parameters.Add("@CurrentLocation", SqlDbType.VarChar, 150)
                .Value = (object?)request.CurrentLocation ?? DBNull.Value;

            command.Parameters.Add("@PurchaseDate", SqlDbType.Date)
                .Value = (object?)request.PurchaseDate ?? DBNull.Value;

            command.Parameters.Add("@RentalEndDate", SqlDbType.Date)
                .Value = (object?)request.RentalEndDate ?? DBNull.Value;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // Actualización de activos
        public async Task UpdateAsync(int id, UpdateAssetRequest request, int performedByUserId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand("usp_Assets_Update", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.Int)
                .Value = id;

            command.Parameters.Add("@AssetCode", SqlDbType.VarChar, 50)
                .Value = request.AssetCode;

            command.Parameters.Add("@SerialNumber", SqlDbType.VarChar, 100)
                .Value = (object?)request.SerialNumber ?? DBNull.Value;

            command.Parameters.Add("@Category", SqlDbType.VarChar, 50)
                .Value = request.Category;

            command.Parameters.Add("@Brand", SqlDbType.VarChar, 100)
                .Value = request.Brand;

            command.Parameters.Add("@Model", SqlDbType.VarChar, 100)
                .Value = (object?)request.Model ?? DBNull.Value;

            command.Parameters.Add("@OwnershipType", SqlDbType.VarChar, 20)
                .Value = request.OwnershipType;

            command.Parameters.Add("@SupplierId", SqlDbType.Int)
                .Value = (object?)request.SupplierId ?? DBNull.Value;

            command.Parameters.Add("@Status", SqlDbType.VarChar, 20)
                .Value = request.Status;

            command.Parameters.Add("@CurrentLocation", SqlDbType.VarChar, 150)
                .Value = (object?)request.CurrentLocation ?? DBNull.Value;

            command.Parameters.Add("@PurchaseDate", SqlDbType.Date)
                .Value = (object?)request.PurchaseDate ?? DBNull.Value;

            command.Parameters.Add("@RentalEndDate", SqlDbType.Date)
                .Value = (object?)request.RentalEndDate ?? DBNull.Value;

            command.Parameters.Add("@PerformedByUserId", SqlDbType.Int)
                .Value = performedByUserId; 


            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // Para asignación de activos a un empleado
        public async Task AssignAsync(int assetId, AssignAssetRequest request, int performedByUserId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand("usp_Assets_Assign", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AssetId", SqlDbType.Int)
                .Value = assetId;

            command.Parameters.Add("@EmployeeId", SqlDbType.Int)
                .Value = request.EmployeeId;

            command.Parameters.Add("@PerformedByUserId", SqlDbType.Int)
                .Value = performedByUserId;

            command.Parameters.Add("@Notes", SqlDbType.VarChar, 500)
                .Value = (object?)request.Notes ?? DBNull.Value;

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // Para regresar un activo
        public async Task ReturnAsync(int assetId, ReturnAssetRequest request, int performedByUserId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand("usp_Assets_Return", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AssetId", SqlDbType.Int)
                .Value = assetId;

            command.Parameters.Add("@PerformedByUserId", SqlDbType.Int)
                .Value = performedByUserId;

            command.Parameters.Add("@ReturnCondition", SqlDbType.VarChar, 250)
                .Value = request.ReturnCondition;

            command.Parameters.Add("@Notes", SqlDbType.VarChar, 500)
                .Value = (object?)request.Notes ?? DBNull.Value;

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // Para el historial de activos
        public async Task<IEnumerable<AssetMovement>> GetHistoryAsync(int assetId)
        {
            var movements = new List<AssetMovement>();

            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "usp_Assets_GetHistory",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@AssetId", SqlDbType.Int)
                .Value = assetId;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                movements.Add(new AssetMovement
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    AssetId = reader.GetInt32(reader.GetOrdinal("AssetId")),

                    MovementType =
                        reader.GetString(reader.GetOrdinal("MovementType")),

                    PreviousStatus =
                        GetNullableString(reader, "PreviousStatus"),

                    NewStatus =
                        GetNullableString(reader, "NewStatus"),

                    PreviousLocation =
                        GetNullableString(reader, "PreviousLocation"),

                    NewLocation =
                        GetNullableString(reader, "NewLocation"),

                    Notes =
                        GetNullableString(reader, "Notes"),

                    CreatedAt =
                        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                    PerformedByUserId =
                        reader.GetInt32(reader.GetOrdinal("PerformedByUserId")),

                    PerformedByUsername =
                        reader.GetString(reader.GetOrdinal("PerformedByUsername"))
                });
            }

            return movements;
        }



        // Helpers
        private static string? GetNullableString(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }

        private static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetInt32(ordinal);
        }

        private static DateTime? GetNullableDateTime(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetDateTime(ordinal);
        }
    }
}