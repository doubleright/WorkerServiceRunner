# WorkerServiceRunner

## 说明

利用.NET Core中的Worker Service，来创建Windows 服务或Linux 守护程序

## Windows 应用

在项目中添加nuget包：[Microsoft.Extensions.Hosting.WindowsServices](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.WindowsServices)

创建服务

```
sc.exe create [ServiceName] binPath=[Service.exe Path]
```

一些相关指令

```
sc.exe query [ServiceName] #查询

sc.exe start [ServiceName] #启动

sc.exe stop  [ServiceName] #停止

sc.exe delete [ServiceName] #移除
```

## Linux 应用

- 添加[Microsoft.Extensions.Hosting.Systemd](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Systemd) NuGet包到项目中，并告诉你的新Worker，其生命周期由systemd管理！

  

## 注意

不要让你的代码阻塞worker类中重写的StartAsync、ExecuteAsync、StopAsync方法。

因为StartAsync方法负责启动Worker Service，如果调用StartAsync方法的线程被一直阻塞了，那么Worker Service的启动就一直完成不了。

同理StopAsync方法负责结束Worker Service，如果调用StopAsync方法的线程被一直阻塞了，那么Worker Service的结束就一直完成不了。

重写时，**不要忘记在worker类中调用base.StartAsync和base.StopAsync**，因为BackgroundService类的StartAsync和StopAsync会执行一些Worker Service的核心代码