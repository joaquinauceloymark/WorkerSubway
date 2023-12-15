using WorkerSubway;
using WorkerSubwayPruebas.Repository.IRepository;
using WorkerSubwayPruebas.Repository;
using Microsoft.EntityFrameworkCore;
using WorkerSubwayPruebas.Data;
using Serilog;

try
{
    var host = Host.CreateDefaultBuilder(args)
        
        .UseWindowsService(options =>
        {
            options.ServiceName = "Worker Subway Clientes";
        })

        .ConfigureServices((hostContext, services) =>
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddHostedService<Worker>();
            services.AddHttpClient();
        })
    .Build();

    await host.RunAsync();
}
catch (Exception)
{

    throw;
}

