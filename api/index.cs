using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Thrift.Api
{
    [Route("api")]
    public class IndexFunction
    {
        [HttpGet]
        [Route("{**catch-all}")]
        public Task<IActionResult> Handle(HttpRequest req)
        {
            // This will delegate all requests to the ASP.NET Core pipeline
            return Task.FromResult<IActionResult>(new OkResult());
        }
    }
}