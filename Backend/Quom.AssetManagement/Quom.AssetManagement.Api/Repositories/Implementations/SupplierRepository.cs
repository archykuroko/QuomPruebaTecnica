using System.Data;
using Microsoft.Data.SqlClient;
using Quom.AssetManagement.Api.Data;
using Quom.AssetManagement.Api.DTOs.Suppliers;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using System.Text.Json;

namespace Quom.AssetManagement.Api.Repositories.Implementations
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SupplierRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Para traer a todos los proovedores
        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            var suppliers = new List<Supplier>();

            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "usp_Suppliers_GetAll",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                suppliers.Add(new Supplier
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),

                    TaxId = GetNullableString(reader, "TaxId"),
                    ContactName = GetNullableString(reader, "ContactName"),
                    Email = GetNullableString(reader, "Email"),
                    Phone = GetNullableString(reader, "Phone"),

                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });
            }

            return suppliers;
        }
        
        // Para crear proovedores
        public async Task<int> CreateAsync(CreateSupplierRequest request)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "usp_Suppliers_Create",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Name", SqlDbType.VarChar, 150)
                .Value = request.Name;

            command.Parameters.Add("@TaxId", SqlDbType.VarChar, 50)
                .Value = (object?)request.TaxId ?? DBNull.Value;

            command.Parameters.Add("@ContactName", SqlDbType.VarChar, 150)
                .Value = (object?)request.ContactName ?? DBNull.Value;

            command.Parameters.Add("@Email", SqlDbType.VarChar, 150)
                .Value = (object?)request.Email ?? DBNull.Value;

            command.Parameters.Add("@Phone", SqlDbType.VarChar, 30)
                .Value = (object?)request.Phone ?? DBNull.Value;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        // Para obtener los servicios del provedor
        public async Task<IEnumerable<ServiceType>> GetServicesAsync(int supplierId)
        {
            var services = new List<ServiceType>();

            await using var connection = _connectionFactory.CreateConnection();
            await using var command =
                new SqlCommand("usp_Suppliers_GetServices", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@SupplierId", SqlDbType.Int)
                .Value = supplierId;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                services.Add(new ServiceType
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name"))
                });
            }

            return services;
        }
        
        // Para establecer servicios
        public async Task SetServicesAsync(
            int supplierId,
            SetSupplierServicesRequest request)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command =
                new SqlCommand("usp_Suppliers_SetServices", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@SupplierId", SqlDbType.Int)
                .Value = supplierId;

            command.Parameters.Add("@ServiceTypeIds", SqlDbType.NVarChar)
                .Value = JsonSerializer.Serialize(request.ServiceTypeIds);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }


        // Helper
        private static string? GetNullableString(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }
    }
}