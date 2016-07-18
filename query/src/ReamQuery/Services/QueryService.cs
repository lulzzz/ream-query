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
            
            var src = _template
                .Replace("##NS##", assmName)
                .Replace("##DB##", "Proxy")
                .Replace("##SCHEMA##", schemaSrc);
                
            var srcLineOffset = -1;
            var lines = src.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            for(var i = lines.Length - 1; i > 0; i--) {
                if (lines[i].Contains(srcToken)) {
                    lines[i] = lines[i].Replace(srcToken, string.Empty);
                    srcLineOffset = i + 1;
                    break;
                }
            }
            var fullSrc = string.Join("\n", lines); // todo: newline constant?
            // the usage of the template should not require mapping the column value
            return new TemplateResponse 
            {
                Template = fullSrc,
                Namespace = assmName,
                ColumnOffset = 0,
                LineOffset = srcLineOffset,
                DefaultQuery = string.Format("{0}.Take(100).Dump();\n\n", schemaResult.DefaultTable)
            };
        }

        string _template = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ReamQuery.Core;
##SCHEMA##
namespace ##NS## 
{
    public static class DumpWrapper
    {
        public static Emitter Emitter;

        public static T Dump<T>(this T o)
        {
            return o.Dump(Emitter);
        }
    }

    public class Main : ##DB##, IGenerated
    {
        public async Task Run(Emitter emitter)
        {
            DumpWrapper.Emitter = emitter;
            await Query();
        }

        async Task Query()
{##SOURCE##}
    }
}
";
    }
}
