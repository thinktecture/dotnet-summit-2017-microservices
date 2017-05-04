using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace OrdersService
{
    class OrdersServiceHost
    {
        private string _baseUrl = "http://localhost:7777";

        private IDisposable _server { get; set; }

        public void Start()
        {
            _server = WebApp.Start<Startup>(_baseUrl);

            Console.WriteLine("Order Service running...");
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
