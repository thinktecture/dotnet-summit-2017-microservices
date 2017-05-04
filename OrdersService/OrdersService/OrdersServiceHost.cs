using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;
using OrdersService.Discovery;

namespace OrdersService
{
    class OrdersServiceHost
    {
        private Uri _baseUrl = new Uri("http://localhost:7777");
        private Uri _healthUrl = new Uri("http://localhost:7777/api/health/ping");

        private static ServiceRegistry _serviceRegistry;
        private static RegistryInformation _registryInformation;
        private static IDisposable _server;
        
        public void Start()
        {
            var registryHost = new ConsulRegistryHost();
            _serviceRegistry = new ServiceRegistry(registryHost);

            Task.Run(async () =>
            {
                _registryInformation = await _serviceRegistry.AddTenantAsync(
                    new CustomWebApiRegistryTenant(_baseUrl), "orders-dns", "1.0.0", _healthUrl);

                _server = WebApp.Start<Startup>(_baseUrl.ToString());
                Console.WriteLine("Orders Service running - listening at {0} ...", _baseUrl);
            }).GetAwaiter().GetResult();

        }

        public void Stop()
        {
            if (_server != null)
            {
                _server.Dispose();
            }

        }
    }
}
