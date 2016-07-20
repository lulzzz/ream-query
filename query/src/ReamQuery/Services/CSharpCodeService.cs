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
    using Microsoft.CodeAnalysis.Text;

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

            var programSource = CodeTemplate
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
            LinePosition position;
            var src = CodeTemplate
                .Replace("##NS##", assmName)
                .ReplaceToken(srcToken, string.Empty, out position);

            return new TemplateResponse 
            {
                Template = src,
                Namespace = assmName,
                LineOffset = position.Line,
                ColumnOffset = position.Character,
            };
        }

        static readonly string CodeTemplate = 
"using System;" + Environment.NewLine + 
"using System.Collections;" + Environment.NewLine + 
"using System.Collections.Generic;" + Environment.NewLine + 
"using System.Linq;" + Environment.NewLine + 
"using System.Reflection;" + Environment.NewLine + 
"using System.Threading;" + Environment.NewLine + 
"using System.Threading.Tasks;" + Environment.NewLine + 
"using ReamQuery.Core;" + Environment.NewLine + 
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
"    public class Main : IGenerated" + Environment.NewLine + 
"    {" + Environment.NewLine + 
"        public async Task Run(Emitter emitter)" + Environment.NewLine + 
"        {" + Environment.NewLine + 
"            DumpWrapper.Emitter = emitter;" + Environment.NewLine + 
"            await ExecuteUserCode();" + Environment.NewLine + 
"        }" + Environment.NewLine + 
"        async Task ExecuteUserCode()" + Environment.NewLine + 
"{##SOURCE##}" + Environment.NewLine + 
"    }" + Environment.NewLine + 
"}";
    }
}
