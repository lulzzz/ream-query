namespace ReamQuery.Services
{
    using System;
    using Microsoft.CodeAnalysis;
    using ReamQuery.Models;
    using NLog;
    using ReamQuery.Core;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class HostService 
    {
        CompileService _compiler;
        ClientService _clientService;

        public HostService(ClientService clientService, CompileService compiler)
        {
            _clientService = clientService;
            _compiler = compiler;
        }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<CompileResult> StartGenerated(Guid id, string source, string assmName, MetadataReference context = null) 
        {
            var sw = new Stopwatch();
            sw.Start();
            var compileResult = _compiler.LoadType(source, assmName, context);
            if (compileResult.Code == Api.StatusCode.Ok)
            {
                using (var emitter = new Emitter(id))
                {
                    var programInstance = (IGenerated) Activator.CreateInstance(compileResult.Type);
                    var e2 = sw.Elapsed.TotalMilliseconds;
                    _clientService.AddEmitter(emitter); // should unsub when disposing
                    sw.Reset();
                    sw.Start();
                    await programInstance.Run(emitter);
                    emitter.Complete();
                    var e3 = sw.Elapsed.TotalMilliseconds;
                    Logger.Debug("IGenerated.Run TotalMilliseconds {0}", sw.Elapsed.TotalMilliseconds);
                }
            }

            return compileResult;
        }
    }
}
