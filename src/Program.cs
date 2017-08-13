using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace DotNetFoundationWebsite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                //(
                //options => {
                //    options.UseHttps("local.pfx","1234");
                //    }
                // )
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                //.UseUrls("http://*:8080", "https://*:43434")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
