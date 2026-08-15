using System.Data;
using Microsoft.Data.SqlClient;
using Quom.AssetManagement.Api.Data;
using Quom.AssetManagement.Api.DTOs.Employees;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;

namespace Quom.AssetManagement.Api.Repositories.Implementations
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public EmployeeRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        // Para obtener todos los empleados
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = new List<Employee>();

            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "usp_Employees_GetAll",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    EmployeeNumber =
                        reader.GetString(reader.GetOrdinal("EmployeeNumber")),
                    FirstName =
                        reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName =
                        reader.GetString(reader.GetOrdinal("LastName")),
                    Email =
                        reader.GetString(reader.GetOrdinal("Email")),
                    Department =
                        reader.IsDBNull(reader.GetOrdinal("Department"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Department")),
                    Location =
                        reader.IsDBNull(reader.GetOrdinal("Location"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Location")),
                    IsActive =
                        reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt =
                        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt =
                        reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });
            }

            return employees;
        }
        // Para crear empleados
        public async Task<int> CreateAsync(CreateEmployeeRequest request)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = new SqlCommand(
                "usp_Employees_Create",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@EmployeeNumber", SqlDbType.VarChar, 50)
                .Value = request.EmployeeNumber;

            command.Parameters.Add("@FirstName", SqlDbType.VarChar, 100)
                .Value = request.FirstName;

            command.Parameters.Add("@LastName", SqlDbType.VarChar, 150)
                .Value = request.LastName;

            command.Parameters.Add("@Email", SqlDbType.VarChar, 150)
                .Value = request.Email;

            command.Parameters.Add("@Department", SqlDbType.VarChar, 100)
                .Value = (object?)request.Department ?? DBNull.Value;

            command.Parameters.Add("@Location", SqlDbType.VarChar, 150)
                .Value = (object?)request.Location ?? DBNull.Value;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }
    }
}