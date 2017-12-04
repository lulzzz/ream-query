
namespace ReamQuery.Server.Helpers
{
    using System;
    using System.Linq;

    public static class ExceptionHelpers
    {
        public static bool ExpectedError(this Exception exn)
        {
            return exn.StatusCode() != Api.StatusCode.UnknownError;
        }

        public static Api.StatusCode StatusCode(this Exception exn) 
        {
            if (IsServerUnreachable(exn))
            {
                return Api.StatusCode.ServerUnreachable;
            }
            else if (IsConnectionStringSyntax(exn))
            {
                return Api.StatusCode.ConnectionStringSyntax;
            }
            return Api.StatusCode.UnknownError;
        }

        static bool IsServerUnreachable(Exception exn)
        {
            var hresults = new [] {
                -2147467259, // The network path was not found
                -2146232060, // sqlserver => Named Pipes Provider, error: 40 - Could not open a connection to SQL Server
                -2146233088, // npgsql => One or more errors occurred. (No such host is known),
                5 // npgsql => No such device or address
            };
            return hresults.Contains(exn.HResult);
        }

        static bool IsConnectionStringSyntax(Exception exn)
        {
            var hresults = new []
            {
                2147024809 // npgsql => Keyword not supported: data source
            }.Select(x => -1 * x);
            return hresults.Contains(exn.HResult);
        }
    }
}