using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceRunner
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IContainer _container;

        //Worker Service���Զ�����ע��Worker���캯����IContainer container����
        public Worker(ILogger<Worker> logger, IContainer container)
        {
            _logger = logger;
            _container = container;
        }

        //Worker Service���Զ�����ע��Worker���캯����ILogger
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        //��дBackgroundService.StartAsync�������ڿ�ʼ�����ʱ��ִ��һЩ�����߼����������ǽ����һ����־
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);

            await base.StartAsync(cancellationToken);
        }

        //��һ�� windows�����linux�ػ����� �Ĵ����߼�����RunTaskOne�����ڲ�������Task�����߳̽��д���ͬ�����ԴӲ���CancellationToken stoppingToken�е�IsCancellationRequested���ԣ���֪Worker Service�����Ƿ��Ѿ���ֹͣ
        protected Task RunTaskOne(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                //�������ֹͣ����ô�����IsCancellationRequested�᷵��true�����Ǿ�Ӧ�ý���ѭ��
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("RunTaskOne running at: {time}", DateTimeOffset.Now);
                    Thread.Sleep(1000);
                }
            }, stoppingToken);
        }

        //�ڶ��� windows�����linux�ػ����� �Ĵ����߼�����RunTaskTwo�����ڲ�������Task�����߳̽��д���ͬ�����ԴӲ���CancellationToken stoppingToken�е�IsCancellationRequested���ԣ���֪Worker Service�����Ƿ��Ѿ���ֹͣ
        protected Task RunTaskTwo(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                //�������ֹͣ����ô�����IsCancellationRequested�᷵��true�����Ǿ�Ӧ�ý���ѭ��
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("RunTaskTwo running at: {time}", DateTimeOffset.Now);
                    Thread.Sleep(1000);
                }
            }, stoppingToken);
        }

        //������ windows�����linux�ػ����� �Ĵ����߼�����RunTaskThree�����ڲ�������Task�����߳̽��д���ͬ�����ԴӲ���CancellationToken stoppingToken�е�IsCancellationRequested���ԣ���֪Worker Service�����Ƿ��Ѿ���ֹͣ
        protected Task RunTaskThree(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                //�������ֹͣ����ô�����IsCancellationRequested�᷵��true�����Ǿ�Ӧ�ý���ѭ��
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("RunTaskThree running at: {time}", DateTimeOffset.Now);
                    Thread.Sleep(1000);
                }
            }, stoppingToken);
        }

        //��дBackgroundService.ExecuteAsync��������װwindows�����linux�ػ������еĴ����߼�
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Task taskOne = RunTaskOne(stoppingToken);
                Task taskTwo = RunTaskTwo(stoppingToken);
                Task taskThree = RunTaskThree(stoppingToken);

                await Task.WhenAll(taskOne, taskTwo, taskThree);//ʹ��await�ؼ��֣��첽�ȴ�RunTaskOne��RunTaskTwo��RunTaskThree�������ص�����Task������ɣ���������ExecuteAsync�������̻߳��������أ����Ῠ�����ﱻ����
            }
            catch (Exception ex)
            {
                //RunTaskOne��RunTaskTwo��RunTaskThree�����У��쳣�����Ĵ����߼����������ǽ����һ����־
                _logger.LogError(ex.Message);
            }
            finally
            {
                //Worker Service����ֹͣ���������Ҫ��β���߼�������д������
            }
        }

        //��дBackgroundService.StopAsync�������ڽ��������ʱ��ִ��һЩ�����߼����������ǽ����һ����־
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);

            await base.StopAsync(cancellationToken);
        }
    }
}
