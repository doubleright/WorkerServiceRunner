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
                .UseWindowsService() //ע�⣬�ڷ� Windows ƽ̨�ϵ��� UseWindowsService ����Ҳ�ǲ��ᱨ��ģ��� Windows ƽ̨����Դ˵��á�
                .UseSystemd() //�� Windows ƽ̨�ϵ��� UseSystemd ����Ҳ�ǲ��ᱨ��ģ�Windows ƽ̨����Դ˵��á�
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddTransient<IContainer, MyContainer>();//����IContainer�ӿں�MyContainer�������ע���ϵ
                    //services.AddSingleton<IContainer, MyContainer>();//����IContainer�ӿں�MyContainer��ĵ���ģʽ
                    services.AddHostedService<Worker>();//����һ��woker
                   
                });
    }
}
