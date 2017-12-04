namespace ReamQuery.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ReamQuery.Server.Models;
    using ReamQuery.Server.Helpers;

    /// <summary>
    /// Provides EF7 based database contexts
    /// </summary>
    public class DatabaseContextService
    {
        SchemaService _schemaService;
        CompileService _compileService;
        IDictionary<string, CompileResult> _map;

        public DatabaseContextService(SchemaService schemaService, CompileService compileService)
        {
            _schemaService = schemaService;
            _compileService = compileService;
            _map = new Dictionary<string, CompileResult>();
        }

        /// <summary>
        /// Returns the database context for the given connection string, 
        /// optionally loading and compiling the types if missing.
        /// </summary>
        public async Task<CompileResult> GetDatabaseContext(string connectionString, DatabaseProviderType type)
        {
            if (!_map.ContainsKey(connectionString))
            {
                var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
                var schemaResult = await _schemaService.GetSchemaSource(connectionString, type, assmName);
                if (schemaResult.Code != Api.StatusCode.Ok)
                {
                    var errRes = new CompileResult { Code = schemaResult.Code };
                    return errRes;
                }
                var result = _compileService.LoadType(schemaResult.Schema, assmName);
                _map.Add(connectionString, result);
            }

            return _map[connectionString];
        }
    }
}
