using System.Data;
using Microsoft.Data.SqlClient;
using Quom.AssetManagement.Api.Data;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;

namespace Quom.AssetManagement.Api.Repositories.Implementations
{
    public class ServiceTypeRepository : IServiceTypeRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public ServiceTypeRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // Para obtener los tipos de servicios de un provedor
        public async Task<IEnumerable<ServiceType>> GetAllAsync()
        {
            var services = new List<ServiceType>();

            await using var connection = _connectionFactory.CreateConnection();
            await using var command =
                new SqlCommand("usp_ServiceTypes_GetAll", connection);

            command.CommandType = CommandType.StoredProcedure;

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
    }
}