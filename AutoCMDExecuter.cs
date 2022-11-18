using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceRunner
{
    public class AutoCMDExecuter : BackgroundService
    {
        public const string NAME = nameof(AutoCMDExecuter);
        private readonly ILogger<AutoCMDExecuter> _logger;
        private readonly IContainer _container;
        private readonly IConfiguration _configuration;
        protected int ExecInterval;
        protected int BATStartHour;
        protected int SYNStartHour;
        protected bool IsExeBATOnce;
        protected bool IsExeSYNOnce;

        public AutoCMDExecuter(ILogger<AutoCMDExecuter> logger, IContainer container)
        {
            _logger = logger;
            _container = container;
        }

        public AutoCMDExecuter(ILogger<AutoCMDExecuter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;          
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(NAME + " starting at: {time}", DateTimeOffset.Now);
            InitParam();
            await base.StartAsync(cancellationToken);
        }

        protected virtual void InitParam() 
        {
            try
            {
                IsExeBATOnce = false;
                IsExeSYNOnce = false;
                ExecInterval = 1;
                SYNStartHour = int.Parse(_configuration[nameof(SYNStartHour)]);
                if (SYNStartHour < 0 || SYNStartHour > 23)
                {
                    SYNStartHour = 0;
                }
                BATStartHour = int.Parse(_configuration[nameof(BATStartHour)]);
                if (BATStartHour < 0 || BATStartHour > 23)
                {
                    BATStartHour = 0;
                }
            }
            catch 
            {
                SYNStartHour = 0;
                BATStartHour = 1;
            }
        }
        
        protected Task RunTaskOne(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {             
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (DateTime.Now.Hour == SYNStartHour && !IsExeSYNOnce)
                        {
                            var syn = _configuration["SYN"] ?? "";
                            var rs = ExeCommand(syn);
                            _logger.LogInformation($"=====================================================\r\n{rs}\r\n=====================================================");
                            IsExeSYNOnce = true;
                        }
                        else if (DateTime.Now.Hour != SYNStartHour)
                        {
                            IsExeSYNOnce = false;
                        }

                        if (DateTime.Now.Hour == BATStartHour && !IsExeBATOnce)
                        {
                            var bat = _configuration["BAT"] ?? "";
                            if (string.IsNullOrWhiteSpace(bat))
                            {
                                throw new Exception("执行CMD脚本路径未配置...");
                            }
                            var cmd = File.ReadAllText(bat);
                            var rs = ExeCommand(cmd);
                            _logger.LogInformation($"=====================================================\r\n{rs}\r\n=====================================================");
                            IsExeBATOnce = true;
                        }
                        else if (DateTime.Now.Hour != BATStartHour)
                        {
                            IsExeBATOnce = false;
                        }                    
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    finally 
                    {
                        Thread.Sleep(ExecInterval * 1000);
                    }                  
                }
            }, stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Task taskOne = RunTaskOne(stoppingToken);

                await Task.WhenAll(taskOne);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }          
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(NAME +" stopping at: {time}", DateTimeOffset.Now);

            await base.StopAsync(cancellationToken);
        }

        private string ExeCommand(string commandText)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            string strOutput;
            try
            {
                p.Start();
                p.StandardInput.WriteLine(commandText);
                p.StandardInput.WriteLine("exit");
                strOutput = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
            }
            catch (Exception e)
            {
                strOutput = e.Message;
            }
            return strOutput;
        }
    }
}
