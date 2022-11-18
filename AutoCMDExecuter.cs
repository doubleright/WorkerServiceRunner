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
        private int ExecInterval;
        private int StartHour;
        private bool IsExeCommandOnce;

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
                IsExeCommandOnce = false;
                ExecInterval = 1;
                StartHour = int.Parse(_configuration[nameof(StartHour)]);
                if (StartHour < 0 || StartHour > 23)
                {
                    StartHour = 0;
                }
            }
            catch 
            {
                StartHour = 0;
                ExecInterval = 1;
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
                        if (DateTime.Now.Hour == StartHour && !IsExeCommandOnce)
                        {
                            var bat = _configuration["BAT"] ?? "";
                            if (string.IsNullOrWhiteSpace(bat))
                            {
                                throw new Exception("执行CMD脚本路径未配置...");
                            }
                            var cmd = File.ReadAllText(bat);
                            var rs = ExeCommand(cmd);
                            _logger.LogInformation($"=====================================================\r\n{rs}\r\n=====================================================");
                            IsExeCommandOnce = true;
                        }
                        else if (DateTime.Now.Hour != StartHour)
                        {
                            IsExeCommandOnce = false;
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
