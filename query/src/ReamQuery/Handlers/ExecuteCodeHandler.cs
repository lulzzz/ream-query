namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;
    using System;

    public class ExecuteCodeHandler : BaseHandler<CodeResponse, CodeRequest>
    {
        CSharpCodeService _service;

        public ExecuteCodeHandler(RequestDelegate next, CSharpCodeService service) : base(next) 
        {
            _service = service; 
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/executecode");
        }

        protected override async Task<CodeResponse> Execute(CodeRequest input)
        {
            if (input.Id == Guid.Empty)
            {
                throw new ArgumentException("Id");
            }
            return await _service.ExecuteCode(input);
        }
    }
}
