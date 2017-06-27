using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SysCommand.ConsoleApp;

namespace SpentBook.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.FirstOrDefault() == "--admin")
            {
                var app = new App();
                var argsList = args.ToList();
                argsList.RemoveAt(0);
                app.Run(argsList.ToArray());
            }
            else {
                BuildWebHost(args).Run();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
