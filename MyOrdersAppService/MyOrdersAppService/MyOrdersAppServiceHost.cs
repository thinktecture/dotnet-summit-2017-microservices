using System;
using Microsoft.Owin.Hosting;

namespace MyOrdersAppService
{
    public class MyOrdersAppServiceHost
    {
        private static IDisposable _server;

        public void Start()
        {
            _server = WebApp.Start<Startup>("http://localhost:4500");
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
