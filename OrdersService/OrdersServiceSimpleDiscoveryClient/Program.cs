using System;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;

namespace SimpleDiscoveryClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceRegistry = new ServiceRegistry(new ConsulRegistryHost());

            var instances = serviceRegistry.FindServiceInstancesAsync("orders").Result;
            foreach (var instance in instances)
            {
                Console.WriteLine($"Address: {instance.Address}:{instance.Port}, Version: {instance.Version}");
            }

            Console.ReadLine();
        }
    }
}
