using WorkerSubway;
using WorkerSubwayPruebas.Repository.IRepository;
using WorkerSubwayPruebas.Repository;
using Microsoft.EntityFrameworkCore;
using WorkerSubwayPruebas.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // Registrar repositorios y otros servicios
        services.AddScoped<IClienteRepository, ClienteRepository>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
