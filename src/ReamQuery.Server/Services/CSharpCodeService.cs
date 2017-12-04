namespace ReamQuery.Server.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using ReamQuery.Server.Api;
    using ReamQuery.Core;
    using ReamQuery.Server.Helpers;
    using System.Linq;
    using NLog;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis.Text;

    public class CSharpCodeService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        FragmentService _fragmentService;
        HostService _hostService;

        public CSharpCodeService(
            FragmentService fragmentService,
            HostService host
        )
        {
            _fragmentService = fragmentService;
            _hostService = host;
        }

        public CodeResponse ExecuteCode(CodeRequest input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var newInput = _fragmentService.Fix(input.Text);
            
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var implName = Guid.NewGuid().ToIdentifierWithPrefix("UserCodeImpl");

            var programSource = CodeTemplate
                .Replace("##SOURCE##", newInput.Text)
                .Replace("##NS##", assmName)
                .Replace("##IMPLNAME##", implName);

            var compileResult = _hostService.StartGenerated(input.Id, programSource, assmName);
            return new CodeResponse
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Diagnostics = compileResult.Diagnostics,
                Code = compileResult.Code
            };
        }

        public TemplateResponse GetTemplate(CodeRequest input) 
        {
            Logger.Debug("{1}: {0}", JsonConvert.SerializeObject(input), input.Id);
            var srcToken = "##SOURCE##";
            var assmName = Guid.NewGuid().ToIdentifierWithPrefix("a");
            var implName = Guid.NewGuid().ToIdentifierWithPrefix("UserCodeImpl");
            var userText = input.Text ?? string.Empty;

            LinePosition position;
            var src = CodeTemplate
                .Replace("##NS##", assmName)
                .Replace("##IMPLNAME##", implName)
                .ReplaceToken(srcToken, userText, out position);

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
"        /// <summary>DumpWrapper.Dump</summary>" + Environment.NewLine +
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
"            await ##IMPLNAME##();" + Environment.NewLine + 
"        }" + Environment.NewLine + 
"        async Task ##IMPLNAME##()" + Environment.NewLine + 
"{##SOURCE##}" + Environment.NewLine + 
"    }" + Environment.NewLine + 
"}";
    }
}
