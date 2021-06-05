using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SEP4_Data.Data;

namespace SEP4_Data
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigService config = new ConfigService();
            int port = config.Port;
            string[] ports;
            if (config.Https)
                ports = new[] {"http://*:" + port, "https://*:" + (port + 1)};
            else
                ports = new[] {"http://*:" + port};
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>().UseUrls(ports); });
            hostBuilder.Build().Run();
        }
    }
}