namespace ReamQuery.Api
{
    public abstract class BaseResponse
    {
        public StatusCode Code { get; set; }

        public string Message { get; set; }
    }
}
