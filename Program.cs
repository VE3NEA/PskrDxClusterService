using VE3NEA.PskrDxClusterService;

IHost host = Host.CreateDefaultBuilder(args)        
  .UseWindowsService(options => { options.ServiceName = "PSKReporter DX Cluster Server"; })        
  .ConfigureServices(services => { services.AddHostedService<WindowsBackgroundService>(); })    
  .Build();

await host.RunAsync();
