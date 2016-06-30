namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Models;
    using System.Threading.Tasks;

    public class QueryTemplateHandler : BaseHandler<TemplateResult, QueryInput>
    {
        QueryService _queryService;

        public QueryTemplateHandler(RequestDelegate next, QueryService queryService) : base(next) 
        {
            _queryService = queryService;
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/querytemplate");
        }

        protected override async Task<TemplateResult> Execute(QueryInput input)
        {
            return await _queryService.GetTemplate(input);
        }
    }
}