using System.Web.Http;

namespace OrdersService.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [Route("ping")]
        public string Ping()
        {
            return "OK";
        }
    }
}
