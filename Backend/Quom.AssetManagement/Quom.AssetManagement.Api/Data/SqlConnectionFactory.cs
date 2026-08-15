using Microsoft.Data.SqlClient;

namespace Quom.AssetManagement.Api.Data
{
    public class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Db")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'Db'.");
        }


        // Crea una nueva conexión por operación.
        // El ciclo de vida de la conexión queda a cargo del Repository que la utilice.
  
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}