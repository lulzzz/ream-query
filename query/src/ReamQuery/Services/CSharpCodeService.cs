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
    using Newtonsoft.Json;

    public class CSharpCodeService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        CompileService _compiler;
        FragmentService _fragmentService;
        ClientService _clientService;

        public CSharpCodeService(CompileService compiler, FragmentService fragmentService, ClientService clients)
        {
            _compiler = compiler;
            _fragmentService = fragmentService;
            _clientService = clients;
        }

        public async Task<CodeResponse> ExecuteCode(CodeRequest input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var newInput = _fragmentService.Fix(input.Text);
            
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");

            var programSource = _template
                .Replace("##SOURCE##", newInput.Text)
                .Replace("##NS##", assmName);

            var e1 = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var compileResult = _compiler.LoadType(programSource, assmName);
            var response = new CodeResponse
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
            }

            return response;
        }

        public TemplateResponse GetTemplate(CodeRequest input) 
        {
            Logger.Debug("{0}", JsonConvert.SerializeObject(input));
            var srcToken = "##SOURCE##";
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var src = _template.Replace("##NS##", assmName);
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
                LineOffset = srcLineOffset
            };
        }

        string _template = @"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ReamQuery.Core;
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

    public class Main : IGenerated
    {
        public async Task Run(Emitter emitter)
        {
            DumpWrapper.Emitter = emitter;
            await ExecuteUserCode();
        }

        async Task ExecuteUserCode()
{##SOURCE##}
    }
}
";
    }
}
