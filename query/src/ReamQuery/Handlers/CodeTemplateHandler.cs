namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;
    using System;

    public class CodeTemplateHandler : BaseHandler<TemplateResponse, CodeRequest>
    {
        CSharpCodeService _codeService;

        public CodeTemplateHandler(RequestDelegate next, CSharpCodeService codeService) : base(next) 
        {
            _codeService = codeService;
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/codetemplate");
        }

        protected override Task<TemplateResponse> Execute(CodeRequest input)
        {
            if (input.Id == Guid.Empty)
            {
                throw new ArgumentException("Id");
            }
            return Task.FromResult(_codeService.GetTemplate(input));
        }
    }
}