namespace ReamQuery.Server.Services
{
    using System;
    using ReamQuery.Server.Models;
    using System.Threading.Tasks;

    public class SchemaService 
    {
        SqlServerSchemaService _sqlServer;

        NpgsqlSchemaService _npgsql;

        SqliteSchemaService _sqlite;

        public SchemaService(SqlServerSchemaService sqlServer, NpgsqlSchemaService npgsql, SqliteSchemaService sqlite) 
        {
            _npgsql = npgsql;
            _sqlServer = sqlServer;
            _sqlite = sqlite;
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
            else if (type == DatabaseProviderType.Sqlite)
            {
                return await _sqlite.GetSchemaSource(connectionString, assemblyNamespace, withUsings);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
