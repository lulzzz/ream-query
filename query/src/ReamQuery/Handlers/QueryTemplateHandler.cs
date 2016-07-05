namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;

    public class QueryTemplateHandler : BaseHandler<TemplateResponse, QueryRequest>
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

        protected override async Task<TemplateResponse> Execute(QueryRequest input)
        {
            return await _queryService.GetTemplate(input);
        }
    }
}