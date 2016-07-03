namespace ReamQuery.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ReamQuery.Models;
    using ReamQuery.Shared;

    public class QueryService
    {
        CompileService _compiler;
        DatabaseContextService _databaseContextService;
        SchemaService _schemaService;
        FragmentService _fragmentService;

        public QueryService(CompileService compiler, DatabaseContextService databaseContextService, SchemaService schemaService, FragmentService fragmentService)
        {
            _compiler = compiler;
            _databaseContextService = databaseContextService;
            _schemaService = schemaService;
            _fragmentService = fragmentService;
        }

        public async Task<IEnumerable<DumpResult>> ExecuteQuery(QueryInput input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var newInput = _fragmentService.Fix(input.Text);
            var contextResult = await _databaseContextService.GetDatabaseContext(input.ConnectionString, input.ServerType);
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var programSource = _template
                .Replace("##SOURCE##", newInput)
                .Replace("##NS##", assmName)
                .Replace("##SCHEMA##", "") // schema is linked
                .Replace("##DB##", contextResult.Type.ToString())
                .Replace("##QUERYID##", Guid.NewGuid().ToString());

            var e1 = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var result = _compiler.LoadType(programSource, assmName, contextResult.Reference);
            var method = result.Type.GetMethod("Run");
            var programInstance = Activator.CreateInstance(result.Type);
            var e2 = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var res = method.Invoke(programInstance, new object[] { }) as IEnumerable<DumpResult>;
            var e3 = sw.Elapsed.TotalMilliseconds;
            //res.Add("Performance", new { DbContext = e1, Loading = e2, Execution = e3 });
            return res;
        }

        public async Task<TemplateResult> GetTemplate(QueryInput input) 
        {
            var srcToken = "##SOURCE##";
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var schemaResult = await _schemaService.GetSchemaSource(input.ConnectionString, input.ServerType, assmName, withUsings: false);
            var schemaSrc = schemaResult.Schema;
            
            var src = _template
                .Replace("##NS##", assmName)
                .Replace("##DB##", "Proxy")
                .Replace("##SCHEMA##", schemaSrc)
                .Replace("##QUERYID##", Guid.NewGuid().ToString());
                
            var srcLineOffset = -1;
            var lines = src.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            for(var i = lines.Length - 1; i > 0; i--) {
                if (lines[i].Contains(srcToken)) {
                    lines[i] = string.Empty;
                    srcLineOffset = i + 1;
                    break;
                }
            }
            var fullSrc = string.Join("\n", lines); // todo: newline constant?
            // the usage of the template should not require mapping the column value
            return new TemplateResult 
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
using ReamQuery.Shared;
##SCHEMA##
namespace ##NS## 
{
    public static class DumpWrapper
    {
        static Guid QueryId = Guid.Parse(""##QUERYID##"");

        public static T Dump<T>(this T o)
        {
            return o.Dump(QueryId);
        }
    }

    public class Main : ##DB##
    {
        public IEnumerable<DumpResult> Run()
        {
            Query();
            var drain = DrainContainer.CloseDrain(Guid.Parse(""##QUERYID##""));
            var result = drain.GetData();
            return result;
        }

        void Query()
        {
##SOURCE##
        }
    }
}
";
    }
}
