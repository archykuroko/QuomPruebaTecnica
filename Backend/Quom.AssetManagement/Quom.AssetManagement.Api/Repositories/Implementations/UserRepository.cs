using System.Data;
using Microsoft.Data.SqlClient;
using Quom.AssetManagement.Api.Data;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;

namespace Quom.AssetManagement.Api.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public UserRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UserAccount?> GetByLoginAsync(string login)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command =
                new SqlCommand("usp_Users_GetByLogin", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Login", SqlDbType.VarChar, 150)
                .Value = login;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new UserAccount
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                FailedLoginAttempts =
                    reader.GetInt32(reader.GetOrdinal("FailedLoginAttempts")),
                LockoutEnd =
                    reader.IsDBNull(reader.GetOrdinal("LockoutEnd"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("LockoutEnd"))
            };
        }


        public async Task RegisterFailedLoginAsync(int userId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command =
                new SqlCommand("usp_Users_RegisterFailedLogin", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ResetLoginAttemptsAsync(int userId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command =
                new SqlCommand("usp_Users_ResetLoginAttempts", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }



    }
}