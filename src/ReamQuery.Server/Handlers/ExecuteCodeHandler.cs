namespace ReamQuery.Server.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Server.Services;
    using ReamQuery.Server.Api;
    using System.Threading.Tasks;
    using System;
    using NLog;

    public class ExecuteCodeHandler : BaseHandler<CodeResponse, CodeRequest>
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        CSharpCodeService _service;

        public ExecuteCodeHandler(RequestDelegate next, CSharpCodeService service) : base(next) 
        {
            _service = service; 
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/executecode");
        }

        protected override Task<CodeResponse> Execute(CodeRequest input)
        {
            if (input.Id == Guid.Empty)
            {
                throw new ArgumentException("Id");
            }
            Logger.Info("{0}:{1}", input.Id, "/executecode");
            return Task.FromResult(_service.ExecuteCode(input));
        }
    }
}
