using System.Web.Http;
using Owin;
using Swashbuckle.Application;

namespace OrdersService
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.EnableSwagger(c=> c.SingleApiVersion("v1", "Orders Service API"))
                .EnableSwaggerUi();

            app.UseWebApi(config);
        }
    }
}
