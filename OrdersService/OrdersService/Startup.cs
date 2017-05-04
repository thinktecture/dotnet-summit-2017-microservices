using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Web.Http;
using System.Web.Http.Cors;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin.Cors;
using OrdersService.AuthZ;
using Owin;
using Swashbuckle.Application;

namespace OrdersService
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.Use(typeof(SignalRAuthorizationMiddleware));

            // TODO: chck this, dude
            app.UseCors(CorsOptions.AllowAll);

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions()
            {
                Authority = "http://localhost:5000",
                RequiredScopes = new List<string>() { "ordersapi"}
            });

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.EnableSwagger(c=> c.SingleApiVersion("v1", "Orders Service API"))
                .EnableSwaggerUi();

            config.Filters.Add(new AuthorizeAttribute());
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            app.UseWebApi(config);
            app.MapSignalR();
        }
    }
}
