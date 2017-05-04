using System.Web.Http;

namespace OrdersService.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        public string Ping()
        {
            return "OK";
        }
    }
}
