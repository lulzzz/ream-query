namespace QueryEngine.Services
{
    using System;
    using QueryEngine.Models;
    using System.Threading.Tasks;

    public class SchemaService 
    {
        SqlServerSchemaService _sqlServer;

        NpgsqlSchemaService _npgsql;

        public SchemaService(SqlServerSchemaService sqlServer, NpgsqlSchemaService npgsql) 
        {
            _npgsql = npgsql;
            _sqlServer = sqlServer;
        }

        public async Task<SchemaResult> GetSchemaSource(string connectionString, DatabaseProviderType type, string assemblyNamespace, bool withUsings = true) 
        {
            if (type == DatabaseProviderType.SqlServer)
            {
                return await _sqlServer.GetSchemaSource(connectionString, assemblyNamespace, withUsings);
            }
            else if (type == DatabaseProviderType.NpgSql)
            {
                return await _npgsql.GetSchemaSource(connectionString, assemblyNamespace, withUsings);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
