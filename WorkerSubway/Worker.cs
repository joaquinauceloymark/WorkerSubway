using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using WorkerSubwayPruebas.Models;
using WorkerSubwayPruebas.Repository.IRepository;

namespace WorkerSubway
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _clientFactory;
        private Timer _timerDailyTask;
        private Timer _timerEveryThirtyMinutes;

        public Worker(ILogger<Worker> logger, IConfiguration config, IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _config = config;
            _scopeFactory = scopeFactory;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_timerDailyTask = new Timer(ExecuteDailyTask, null, GetNextRunTime(), TimeSpan.FromDays(_config.GetValue<int>("EjecutionTime:Daily")));
            _timerEveryThirtyMinutes = new Timer(ExecuteThirtyMinuteTask, null, TimeSpan.Zero, TimeSpan.FromMinutes(_config.GetValue<int>("EjecutionTime:Instant")));

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private void ExecuteDailyTask(object state)
        {
            _logger.LogInformation("Ejecutando tarea diaria 6am.");
            try
            {
                var client = _clientFactory.CreateClient();
                var response = client.GetAsync(_config.GetValue<string>("ConnectionStrings:EndPointAPI")).Result;
                _logger.LogInformation("Response de llamado a API", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Clientes actualizados a las 6 AM");
                }
                else
                {
                    _logger.LogError($"Error al actualizar clientes a las 6 AM: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en la tarea diaria: {ex.Message}");
            }
        }

        private void ExecuteThirtyMinuteTask(object state)
        {
            _logger.LogInformation("Ejecutando tarea cada 30 minutos.");
            try
            {
                var client = _clientFactory.CreateClient();
                var response = client.GetAsync(_config.GetValue<string>("ConnectionStrings:EndPointAPIGetAll")).Result;

                _logger.LogInformation("Response de llamado a API", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        var clientes = JsonConvert.DeserializeObject<List<cli_clientes>>(content);
                        var clienteRepository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();

                        foreach (var cliente in clientes)
                        {
                            
                            cliente.CLI_PROCESADO = "N";
                            clienteRepository.UpdateCliProcesado(cliente);

                            var clienteCis = clienteRepository.GetCisClienteByCedula(cliente.CLI_NUMTARJETAACTIVA);
                            if (clienteCis != null)
                            {
                                if (cliente.CLI_PUNTOSDISPONIBLES != decimal.Parse(clienteCis.Saldo.ToString()))
                                    clienteRepository.UpdateSaldoCisCliente(clienteCis.Cedula, cliente.CLI_PUNTOSDISPONIBLES);
                            }
                            else
                            {
                                clientes cli = new clientes();
                                cli.Cia = "1";
                                cli.Codigo = !string.IsNullOrEmpty(cliente.CLI_CODIGOENCLIENTE) ? cliente.CLI_CODIGOENCLIENTE : _config.GetValue<string>("ValoresDefectoCliCliente:Codigo");
                                cli.Renta = _config.GetValue<int>("ValoresDefectoCliCliente:Renta");
                                cli.Nombre = cliente.CLI_NOMBRE;
                                cli.Telefono = !string.IsNullOrEmpty(cliente.CLI_TELEFONOCELULAR) ? cliente.CLI_TELEFONOCELULAR : _config.GetValue<string>("ValoresDefectoCliCliente:Telefono");
                                cli.Cedula = !string.IsNullOrEmpty(cliente.CLI_CEDULA) ? cliente.CLI_CEDULA : _config.GetValue<string>("ValoresDefectoCliCliente:Cedula");
                                cli.Apartado = _config.GetValue<string>("ValoresDefectoCliCliente:Apartado");
                                cli.F_ult_compra = DateTime.Now;
                                cli.Fax = !string.IsNullOrEmpty(cliente.CLI_FAX) ? cliente.CLI_FAX : _config.GetValue<string>("ValoresDefectoCliCliente:Fax");
                                cli.Plazo = _config.GetValue<short>("ValoresDefectoCliCliente:Plazo");
                                cli.C_contable = _config.GetValue<string>("ValoresDefectoCliCliente:C_contable");
                                cli.Dia_cobro = _config.GetValue<short>("ValoresDefectoCliCliente:Dia_cobro");
                                cli.Exento = _config.GetValue<int>("ValoresDefectoCliCliente:Exento");
                                cli.IV = _config.GetValue<float>("ValoresDefectoCliCliente:IV");
                                cli.Saldo = float.Parse(cliente.CLI_PUNTOSDISPONIBLES.ToString());
                                cli.Linea1 = _config.GetValue<string>("ValoresDefectoCliCliente:Linea1");
                                cli.Saldo_Ant = _config.GetValue<float>("ValoresDefectoCliCliente:Saldo_Ant");
                                cli.Linea2 = _config.GetValue<string>("ValoresDefectoCliCliente:Linea2");
                                cli.Aplica = _config.GetValue<decimal>("ValoresDefectoCliCliente:Aplica");
                                cli.Adelantos = _config.GetValue<float>("ValoresDefectoCliCliente:Adelantos");
                                cli.email = !string.IsNullOrEmpty(cliente.CLI_EMAIL) ? cliente.CLI_EMAIL : _config.GetValue<string>("ValoresDefectoCliCliente:Email");
                                cli.Limi_cred = _config.GetValue<float>("ValoresDefectoCliCliente:Limi_cred");
                                cli.Zona = !string.IsNullOrEmpty(cliente.CLI_CODIGOPOSTAL) ? cliente.CLI_CODIGOPOSTAL : _config.GetValue<string>("ValoresDefectoCliCliente:Zona");
                                cli.Agente = !string.IsNullOrEmpty(cliente.CREATEUSERID) ? cliente.CREATEUSERID.Substring(0, 2) : _config.GetValue<string>("ValoresDefectoCliCliente:Agente");
                                cli.Contacto = _config.GetValue<string>("ValoresDefectoCliCliente:Contacto");
                                cli.Moneda = _config.GetValue<string>("ValoresDefectoCliCliente:Moneda");
                                cli.Direccion = _config.GetValue<string>("ValoresDefectoCliCliente:Direccion");
                                cli.Dia_Tramit = _config.GetValue<short>("ValoresDefectoCliCliente:Dia_Tramit");
                                cli.Descuento = _config.GetValue<float>("ValoresDefectoCliCliente:Descuento");
                                cli.NumPrecio = _config.GetValue<short>("ValoresDefectoCliCliente:NumPrecio");
                                cli.ExentoRenta = _config.GetValue<short>("ValoresDefectoCliCliente:ExentoRenta");
                                cli.Credito = _config.GetValue<short>("ValoresDefectoCliCliente:Credito");
                                cli.Comision = _config.GetValue<float>("ValoresDefectoCliCliente:Comision");

                                var response2 = clienteRepository.InsertCisCliente(cli);
                            }
                        }
                    }


                    _logger.LogInformation("Clientes actualizados a los 30 minutos");
                }
                else
                {
                    _logger.LogError($"Error al actualizar clientes a los 30 minutos: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en la tarea diaria: {ex.Message}");
            }
        }

        private TimeSpan GetNextRunTime()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(6);
            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }
            return nextRun - now;
        }

        public override void Dispose()
        {
            _timerDailyTask?.Dispose();
            _timerEveryThirtyMinutes?.Dispose();
            base.Dispose();
        }
    }


    /*public void GetClientes()
    {
        try
        {

            using (var scope = _scopeFactory.CreateScope())
            {
                var clienteRepository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();


                var respuesta = clienteRepository.GetClientes();

                foreach (var cliente in respuesta)
                {
                    cliente.CLI_PROCESADO = "N";
                    clienteRepository.UpdateCliProcesado(cliente);
                    var c = clienteRepository.GetUltimoCliente();
                    var ciaInt = 1;
                    if (c != null)
                    {
                        ciaInt = Convert.ToInt32(c);
                        ciaInt = ciaInt + 1;
                    }

                    var clienteCis = clienteRepository.GetCisClienteByCedula(cliente.CLI_CEDULA);
                    if (clienteCis != null)
                    {
                        if (cliente.CLI_PUNTOSDISPONIBLES != decimal.Parse(clienteCis.Saldo.ToString()))
                            clienteRepository.UpdateSaldoCisCliente(clienteCis.Cedula, cliente.CLI_PUNTOSDISPONIBLES);
                    }
                    else
                    {
                        clientes cli = new clientes();
                        cli.Cia = (ciaInt).ToString();
                        cli.Codigo = !string.IsNullOrEmpty(cliente.CLI_CODIGOENCLIENTE) ? cliente.CLI_CODIGOENCLIENTE : _config.GetValue<string>("ValoresDefectoCliCliente:Codigo");
                        cli.Renta = _config.GetValue<int>("ValoresDefectoCliCliente:Renta");
                        cli.Nombre = cliente.CLI_NOMBRE;
                        cli.Telefono = !string.IsNullOrEmpty(cliente.CLI_TELEFONOCELULAR) ? cliente.CLI_TELEFONOCELULAR : _config.GetValue<string>("ValoresDefectoCliCliente:Telefono");
                        cli.Cedula = !string.IsNullOrEmpty(cliente.CLI_CEDULA) ? cliente.CLI_CEDULA : _config.GetValue<string>("ValoresDefectoCliCliente:Cedula");
                        cli.Apartado = _config.GetValue<string>("ValoresDefectoCliCliente:Apartado");
                        cli.F_ult_compra = DateTime.Now;
                        cli.Fax = !string.IsNullOrEmpty(cliente.CLI_FAX) ? cliente.CLI_FAX : _config.GetValue<string>("ValoresDefectoCliCliente:Fax");
                        cli.Plazo = _config.GetValue<short>("ValoresDefectoCliCliente:Plazo");
                        cli.C_contable = _config.GetValue<string>("ValoresDefectoCliCliente:C_contable");
                        cli.Dia_cobro = _config.GetValue<short>("ValoresDefectoCliCliente:Dia_cobro");
                        cli.Exento = _config.GetValue<int>("ValoresDefectoCliCliente:Exento");
                        cli.IV = _config.GetValue<float>("ValoresDefectoCliCliente:IV");
                        cli.Saldo = float.Parse(cliente.CLI_PUNTOSDISPONIBLES.ToString());
                        cli.Linea1 = _config.GetValue<string>("ValoresDefectoCliCliente:Linea1");
                        cli.Saldo_Ant = _config.GetValue<float>("ValoresDefectoCliCliente:Saldo_Ant");
                        cli.Linea2 = _config.GetValue<string>("ValoresDefectoCliCliente:Linea2");
                        cli.Aplica = _config.GetValue<decimal>("ValoresDefectoCliCliente:Aplica");
                        cli.Adelantos = _config.GetValue<float>("ValoresDefectoCliCliente:Adelantos");
                        cli.email = !string.IsNullOrEmpty(cliente.CLI_EMAIL) ? cliente.CLI_EMAIL : _config.GetValue<string>("ValoresDefectoCliCliente:Email");
                        cli.Limi_cred = _config.GetValue<float>("ValoresDefectoCliCliente:Limi_cred");
                        cli.Zona = !string.IsNullOrEmpty(cliente.CLI_CODIGOPOSTAL) ? cliente.CLI_CODIGOPOSTAL : _config.GetValue<string>("ValoresDefectoCliCliente:Zona");
                        cli.Agente = !string.IsNullOrEmpty(cliente.CREATEUSERID) ? cliente.CREATEUSERID.Substring(0, 2) : _config.GetValue<string>("ValoresDefectoCliCliente:Agente");
                        cli.Contacto = _config.GetValue<string>("ValoresDefectoCliCliente:Contacto");
                        cli.Moneda = _config.GetValue<string>("ValoresDefectoCliCliente:Moneda");
                        cli.Direccion = _config.GetValue<string>("ValoresDefectoCliCliente:Direccion");
                        cli.Dia_Tramit = _config.GetValue<short>("ValoresDefectoCliCliente:Dia_Tramit");
                        cli.Descuento = _config.GetValue<float>("ValoresDefectoCliCliente:Descuento");
                        cli.NumPrecio = _config.GetValue<short>("ValoresDefectoCliCliente:NumPrecio");
                        cli.ExentoRenta = _config.GetValue<short>("ValoresDefectoCliCliente:ExentoRenta");
                        cli.Credito = _config.GetValue<short>("ValoresDefectoCliCliente:Credito");
                        cli.Comision = _config.GetValue<float>("ValoresDefectoCliCliente:Comision");

                        var response2 = clienteRepository.InsertCisCliente(cli);
                    }


                }
            }
        }
        catch (Exception e)
        {

            throw;
        }
    }
    */
}