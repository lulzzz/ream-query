namespace ReamQuery.Services
{
    using System;
    using System.Threading.Tasks;
    using ReamQuery.Api;
    using ReamQuery.Helpers;
    using System.Linq;
    using NLog;
    using Microsoft.CodeAnalysis.Text;

    public class QueryService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        DatabaseContextService _databaseContextService;
        SchemaService _schemaService;
        FragmentService _fragmentService;
        HostService _hostService;

        public QueryService(DatabaseContextService databaseContextService,
            SchemaService schemaService, FragmentService fragmentService, HostService host)
        {
            _databaseContextService = databaseContextService;
            _schemaService = schemaService;
            _fragmentService = fragmentService;
            _hostService = host;
        }

        public async Task<QueryResponse> ExecuteQuery(QueryRequest input)
        {
            var newInput = _fragmentService.Fix(input.Text);
            var contextResult = await _databaseContextService.GetDatabaseContext(input.ConnectionString, input.ServerType);
            if (contextResult.Code != Api.StatusCode.Ok)
            {
                return new QueryResponse { Code = contextResult.Code, Message = contextResult.Message };
            }
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var implName = Guid.NewGuid().ToIdentifierWithPrefix("UserCodeImpl");
            var programSource = CodeTemplate
                .Replace("##NS##", assmName)
                .Replace("##SCHEMA##", "") // schema is linked
                .Replace("##DB##", contextResult.Type.ToString())
                .Replace("##IMPLNAME##", implName)
                .Replace("##SOURCE##", newInput.Text);

            var compileResult = _hostService.StartGenerated(input.Id, programSource, assmName, contextResult.Reference);
            return new QueryResponse
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Diagnostics = compileResult.Diagnostics,
                Code = compileResult.Code
            };
        }

        public async Task<TemplateResponse> GetTemplate(QueryRequest input) 
        {
            var srcToken = "##SOURCE##";
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var implName = Guid.NewGuid().ToIdentifierWithPrefix("UserCodeImpl");
            var schemaResult = await _schemaService.GetSchemaSource(input.ConnectionString, input.ServerType, assmName, withUsings: false);
            var schemaSrc = schemaResult.Schema;
            var userText = input.Text ?? string.Empty;
            
            LinePosition tokenPos;
            var src = CodeTemplate
                .Replace("##NS##", assmName)
                .Replace("##DB##", "Proxy")
                .Replace("##SCHEMA##", schemaSrc)
                .Replace("##IMPLNAME##", implName)
                .ReplaceToken(srcToken, userText, out tokenPos);

            return new TemplateResponse 
            {
                Template = src,
                Namespace = assmName,
                LineOffset = tokenPos.Line,
                ColumnOffset = tokenPos.Character,
                DefaultQuery = string.Format("{0}.Take(100).Dump();{1}{1}", schemaResult.DefaultTable, Environment.NewLine)
            };
        }

        static readonly string CodeTemplate = "using System;" + Environment.NewLine +
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
// "    public class Xoo : ReamQuery.Services.TestClass { } " + 
"    public static class DumpWrapper" + Environment.NewLine +
"    {" + Environment.NewLine +
"        public static Emitter Emitter;" + Environment.NewLine +
"        /// <summary>DumpWrapper.Dump</summary>" + Environment.NewLine +
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
"            await ##IMPLNAME##();" + Environment.NewLine +
"        }" + Environment.NewLine +
"        async Task ##IMPLNAME##()" + Environment.NewLine +
"{##SOURCE##}" + Environment.NewLine +
"    }" + Environment.NewLine +
"}"
;
    }

    public class TestClass
    {

    }
}
