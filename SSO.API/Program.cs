using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SSO.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            IWebHost webHost = WebHost
                              .CreateDefaultBuilder(args)
                              .UseStartup<Startup>()
                              .Build();
            return webHost;
        }
    }
}
