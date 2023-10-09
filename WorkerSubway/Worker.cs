using WorkerSubwayPruebas.Models;
using WorkerSubwayPruebas.Repository.IRepository;

namespace WorkerSubway
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;


        public Worker(ILogger<Worker> logger, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _config = config;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                GetClientes();

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void GetClientes()
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
                        cliente.CLI_NUMTARJETAACTIVA = "";
                        clienteRepository.UpdateCliProcesado(cliente);

                        //Obtengo el ultimo cliente registrado para saber que valor colocar en CIA.
                        var c = clienteRepository.GetUltimoCliente();
                        var ciaInt = 0;
                        if (c != null)
                        {
                            ciaInt = Convert.ToInt32(c);
                            ciaInt = ciaInt + 1;
                        }


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
                        cli.Plazo = _config.GetValue<int>("ValoresDefectoCliCliente:Plazo");
                        cli.C_contable = _config.GetValue<string>("ValoresDefectoCliCliente:C_contable");
                        cli.Dia_cobro = _config.GetValue<int>("ValoresDefectoCliCliente:Dia_cobro");
                        cli.Exento = _config.GetValue<int>("ValoresDefectoCliCliente:Exento");
                        cli.IV = _config.GetValue<float>("ValoresDefectoCliCliente:IV");
                        cli.Saldo = _config.GetValue<float>("ValoresDefectoCliCliente:Saldo");
                        cli.Linea1 = _config.GetValue<string>("ValoresDefectoCliCliente:Linea1");
                        cli.Saldo_Ant = _config.GetValue<float>("ValoresDefectoCliCliente:Saldo_Ant");
                        cli.Linea2 = _config.GetValue<string>("ValoresDefectoCliCliente:Linea2");
                        cli.Aplica = _config.GetValue<float>("ValoresDefectoCliCliente:Aplica");
                        cli.Adelantos = _config.GetValue<float>("ValoresDefectoCliCliente:Adelantos");
                        cli.email = !string.IsNullOrEmpty(cliente.CLI_EMAIL) ? cliente.CLI_EMAIL : _config.GetValue<string>("ValoresDefectoCliCliente:Email");
                        cli.Limi_cred = _config.GetValue<float>("ValoresDefectoCliCliente:Limi_cred");
                        cli.Zona = !string.IsNullOrEmpty(cliente.CLI_CODIGOPOSTAL) ? cliente.CLI_CODIGOPOSTAL : _config.GetValue<string>("ValoresDefectoCliCliente:Zona");
                        cli.Agente = !string.IsNullOrEmpty(cliente.CREATEUSERID) ? cliente.CREATEUSERID.Substring(0, 2) : _config.GetValue<string>("ValoresDefectoCliCliente:Agente");
                        cli.Contacto = _config.GetValue<string>("ValoresDefectoCliCliente:Contacto");
                        cli.Moneda = _config.GetValue<string>("ValoresDefectoCliCliente:Moneda");
                        cli.Direccion = _config.GetValue<string>("ValoresDefectoCliCliente:Direccion");
                        cli.Dia_Tramit = _config.GetValue<int>("ValoresDefectoCliCliente:Dia_Tramit");
                        cli.Descuento = _config.GetValue<float>("ValoresDefectoCliCliente:Descuento");
                        cli.NumPrecio = _config.GetValue<int>("ValoresDefectoCliCliente:NumPrecio");
                        cli.ExentoRenta = _config.GetValue<int>("ValoresDefectoCliCliente:ExentoRenta");
                        cli.Credito = _config.GetValue<int>("ValoresDefectoCliCliente:Credito");
                        cli.Comision = _config.GetValue<float>("ValoresDefectoCliCliente:Comision");

                        var response2 = clienteRepository.InsertCisCliente(cli);
                    }
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}