namespace ReamQuery.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using ReamQuery.Api;
    using ReamQuery.Core;
    using ReamQuery.Helpers;
    using System.Linq;
    using NLog;
    using Microsoft.CodeAnalysis.Text;

    public class QueryService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        CompileService _compiler;
        DatabaseContextService _databaseContextService;
        SchemaService _schemaService;
        FragmentService _fragmentService;
        ClientService _clientService;

        public QueryService(CompileService compiler, DatabaseContextService databaseContextService,
            SchemaService schemaService, FragmentService fragmentService, ClientService clients)
        {
            _compiler = compiler;
            _databaseContextService = databaseContextService;
            _schemaService = schemaService;
            _fragmentService = fragmentService;
            _clientService = clients;
        }

        public async Task<QueryResponse> ExecuteQuery(QueryRequest input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var newInput = _fragmentService.Fix(input.Text);
            var contextResult = await _databaseContextService.GetDatabaseContext(input.ConnectionString, input.ServerType);
            if (contextResult.Code != Api.StatusCode.Ok)
            {
                return new QueryResponse { Code = contextResult.Code, Message = contextResult.Message };
            }
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");

            var programSource = _template
                .Replace("##SOURCE##", newInput.Text)
                .Replace("##NS##", assmName)
                .Replace("##SCHEMA##", "") // schema is linked
                .Replace("##DB##", contextResult.Type.ToString());

            var e1 = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var compileResult = _compiler.LoadType(programSource, assmName, contextResult.Reference);
            var queryResponse = new QueryResponse
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Diagnostics = compileResult.Diagnostics,
                Code = compileResult.Code
            };

            if (compileResult.Code == Api.StatusCode.Ok)
            {
                var method = compileResult.Type.GetMethod("Run");
                var programInstance = (IGenerated) Activator.CreateInstance(compileResult.Type);
                var e2 = sw.Elapsed.TotalMilliseconds;
                var emitter = new Emitter(input.Id, newInput.ExpressionLocations.Count());
                _clientService.AddEmitter(emitter);
                sw.Reset();
                sw.Start();
                await programInstance.Run(emitter);
                var e3 = sw.Elapsed.TotalMilliseconds;
                Logger.Debug("IGenerated.Run TotalMilliseconds {0}", sw.Elapsed.TotalMilliseconds);
            }

            return queryResponse;
        }

        public async Task<TemplateResponse> GetTemplate(QueryRequest input) 
        {
            var srcToken = "##SOURCE##";
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var schemaResult = await _schemaService.GetSchemaSource(input.ConnectionString, input.ServerType, assmName, withUsings: false);
            var schemaSrc = schemaResult.Schema;
            
            LinePosition tokenPos;
            var src = _template
                .Replace("##NS##", assmName)
                .Replace("##DB##", "Proxy")
                .Replace("##SCHEMA##", schemaSrc)
                .ReplaceToken(srcToken, string.Empty, out tokenPos);

            return new TemplateResponse 
            {
                Template = src,
                Namespace = assmName,
                LineOffset = tokenPos.Line,
                ColumnOffset = tokenPos.Character,
                DefaultQuery = string.Format("{0}.Take(100).Dump();{1}{1}", schemaResult.DefaultTable, Environment.NewLine)
            };
        }

        string _template = "using System;" + Environment.NewLine +
"using System.Collections;" + Environment.NewLine +
"using System.Collections.Generic;" + Environment.NewLine +
"using System.Linq;" + Environment.NewLine +
"using System.Reflection;" + Environment.NewLine +
"using System.Threading;" + Environment.NewLine +
"using System.Threading.Tasks;" + Environment.NewLine +
"using System.ComponentModel.DataAnnotations;" + Environment.NewLine +
"using System.ComponentModel.DataAnnotations.Schema;" + Environment.NewLine +
"using Microsoft.EntityFrameworkCore;" + Environment.NewLine +
"using Microsoft.EntityFrameworkCore.Metadata;" + Environment.NewLine +
"using ReamQuery.Core;" + Environment.NewLine +
"##SCHEMA##" + Environment.NewLine +
"namespace ##NS##" + Environment.NewLine +
"{" + Environment.NewLine +
"    public static class DumpWrapper" + Environment.NewLine +
"    {" + Environment.NewLine +
"        public static Emitter Emitter;" + Environment.NewLine +
"        public static T Dump<T>(this T o)" + Environment.NewLine +
"        {" + Environment.NewLine +
"            return o.Dump(Emitter);" + Environment.NewLine +
"        }" + Environment.NewLine +
"    }" + Environment.NewLine +
"    public class Main : ##DB##, IGenerated" + Environment.NewLine +
"    {" + Environment.NewLine +
"        public async Task Run(Emitter emitter)" + Environment.NewLine +
"        {" + Environment.NewLine +
"            DumpWrapper.Emitter = emitter;" + Environment.NewLine +
"            await Query();" + Environment.NewLine +
"        }" + Environment.NewLine +
"        async Task Query()" + Environment.NewLine +
"{##SOURCE##}" + Environment.NewLine +
"    }" + Environment.NewLine +
"}"
;
    }
}
