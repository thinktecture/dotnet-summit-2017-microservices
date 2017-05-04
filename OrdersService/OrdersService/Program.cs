using Topshelf;

namespace OrdersService
{
    public class Program
    {
        public static void Main()
        {
            HostFactory.Run(s =>
            {
                s.Service<OrdersServiceHost>(svc =>
                {
                    svc.ConstructUsing(name => new OrdersServiceHost());
                    svc.WhenStarted(tc => tc.Start());
                    svc.WhenStopped(tc => tc.Stop());
                });

                s.RunAsNetworkService();

                s.SetDisplayName("My Cool Orders Service");
                s.SetDescription("WTF!?");
                s.SetServiceName("OrderService");
            });
        }
    }
}
