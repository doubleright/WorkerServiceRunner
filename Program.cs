using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerServiceRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() //注意，在非 Windows 平台上调用 UseWindowsService 方法也是不会报错的，非 Windows 平台会忽略此调用。
                .UseSystemd() //在 Windows 平台上调用 UseSystemd 方法也是不会报错的，Windows 平台会忽略此调用。
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddTransient<IContainer, MyContainer>();//配置IContainer接口和MyContainer类的依赖注入关系
                    //services.AddSingleton<IContainer, MyContainer>();//配置IContainer接口和MyContainer类的单列模式
                    services.AddHostedService<Worker>();//增加一个woker
                   
                });
    }
}
