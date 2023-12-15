using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Text;
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
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


        public Worker(ILogger<Worker> logger, IConfiguration config, IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _config = config;
            _scopeFactory = scopeFactory;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configuración del timer para la tarea diaria
            _timerDailyTask = new Timer(
                async (state) => await ExecuteDailyTaskWrapperAsync(state, stoppingToken),
                null,
                GetNextRunTime(),
                TimeSpan.FromDays(_config.GetValue<int>("EjecutionTime:Daily")));

            // Configuración del timer para la tarea de cada 30 minutos
            _timerEveryThirtyMinutes = new Timer(
                async (state) => await ExecuteThirtyMinuteTaskWrapperAsync(state, stoppingToken),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(_config.GetValue<int>("EjecutionTime:Instant")));

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private async Task ExecuteDailyTaskWrapperAsync(object state, CancellationToken stoppingToken)
        {
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                await ExecuteDailyTask(stoppingToken);
            }
            catch (Exception ex)
            {
                LogError($"Error en Ejecucion de tarea diaria: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ExecuteThirtyMinuteTaskWrapperAsync(object state, CancellationToken stoppingToken)
        {
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                await ExecuteThirtyMinuteTask(stoppingToken);
            }
            catch (Exception ex)
            {
                LogError($"Error en de tarea: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ExecuteDailyTask(object state)
        {
            Log("Iniciando Proceso 6AM");
            _logger.LogInformation("Ejecutando tarea diaria 6am.");
            try
            {
                var client = _clientFactory.CreateClient();
                var response = client.GetAsync(_config.GetValue<string>("ConnectionStrings:EndPointAPI")).Result;
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

                            //cliente.CLI_PROCESADO = "N";
                            //string cliIdentificacion = cliente.CLI_IDENTIFICACION.ToString();

                            //// Crear HttpContent a partir de la cadena
                            //HttpContent clienteIdentificacion = new StringContent(cliIdentificacion, Encoding.UTF8, "application/json");

                            //// Enviar la solicitud POST
                            //HttpResponseMessage responseUpdatecliCliente = client.PostAsync(_config.GetValue<string>("ConnectionStrings:EndPointUpdateCliCliente"), clienteIdentificacion).Result;

                            var clienteCis = clienteRepository.GetCisClienteByCedula(cliente.CLI_NUMTARJETAACTIVA);
                            if (clienteCis != null)
                            {
                                if (cliente.CLI_PUNTOSDISPONIBLES != decimal.Parse(clienteCis.Saldo.ToString()))
                                    clienteRepository.UpdateSaldoCisCliente(clienteCis.Codigo, cliente.CLI_PUNTOSDISPONIBLES);
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
                    Log($"Finalizacion proceso actualizacion diario");

                    SendSuccessEmail(false);

                    _logger.LogInformation("Clientes actualizados.");
                }
                else
                {
                    if (_config.GetValue<bool>("Mail:enabled"))
                    {
                        SendErrorEmail($"Error en la tarea diaria: {response.StatusCode}");
                        LogError($"Error al actualizar clientes, tarea diaria: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                SendErrorEmail($"Error en la tarea diaria: {ex.ToString()}");
                LogError($"Error en la tarea diaria: {ex.InnerException.ToString()}");
            }
        }

        private async Task ExecuteThirtyMinuteTask(object state)
        {
            Log($"Iniciando Proceso cada {_config.GetValue<int>("EjecutionTime:Instant")} minutos");
            _logger.LogInformation("Ejecutando tarea cada 30 minutos.");
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync(_config.GetValue<string>("ConnectionStrings:EndPointAPIGetAll"));

                _logger.LogInformation("Response de llamado a API", response.StatusCode);
                Log("Llamado de API exitoso");

                if (response.IsSuccessStatusCode)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var content = await  response.Content.ReadAsStringAsync();
                        var clientes = JsonConvert.DeserializeObject<List<cli_clientes>>(content);
                        var clienteRepository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();

                        foreach (var cliente in clientes)
                        {
                            var clienteCis = clienteRepository.GetCisClienteByCedula(cliente.CLI_NUMTARJETAACTIVA);
                            if (clienteCis != null)
                            {
                                if (cliente.CLI_PUNTOSDISPONIBLES != decimal.Parse(clienteCis.Saldo.ToString()))
                                {
                                    clienteRepository.UpdateSaldoCisCliente(clienteCis.Codigo, cliente.CLI_PUNTOSDISPONIBLES);
                                    Log($"Cliente {clienteCis.Codigo} con puntos actualizados");
                                }
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
                                if (response2)
                                {
                                    Log($"Cliente insertado {cli.Codigo}");
                                }
                            }

                        }
                    }

                    Log($"Finalizacion proceso actualizacion {_config.GetValue<int>("EjecutionTime:Instant")} minutos.");
                    if (_config.GetValue<bool>("Mail:enabledSuccessMail"))
                    {
                        await SendSuccessEmail(true);
                    }

                    _logger.LogInformation("Clientes actualizados tarea instantanea");
                }
                else
                {
                    await SendErrorEmail($"Error en la tarea diaria: {response.StatusCode}");
                    LogError($"Error al actualizar clientes instantaneo: {response}");
                }
            }
            catch (Exception ex)
            {
                SendErrorEmail($"Error en la tarea diaria: {ex.ToString()}");
                LogError($"Error en la tarea diaria: {ex.InnerException.ToString()}");
            }
        }

        private async Task SendErrorEmail(string errorMessage)
        {
            MailMessage mail = new MailMessage();
            string SMTP = _config.GetValue<string>("Mail:smtp");
            string port = _config.GetValue<string>("Mail:port");
            string user = _config.GetValue<string>("Mail:user");
            string password = _config.GetValue<string>("Mail:password");

            mail.From = new MailAddress(_config.GetValue<string>("Mail:mailFrom"));
            string emails = _config.GetValue<string>("Mail:mailToError");
            string[] emailList = emails.Split(',');

            // Agregar cada correo al destinatario
            foreach (string email in emailList)
            {
                mail.To.Add(email.Trim());
            }

            mail.Subject = "Estado critico worker puntos de clientes";
            mail.Body = $"Ha ocurrido un error sincronizacion de puntos en subway {_config.GetValue<string>("Tienda:Nombre")}: {errorMessage}";
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.Normal;
            var credentials = new NetworkCredential(user, password);
            SmtpClient smtp = new SmtpClient(SMTP, Convert.ToInt32(port));
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = credentials;
            smtp.Host = SMTP;
            smtp.Port = Convert.ToInt32(port);
            smtp.Send(mail);
        }

        private async Task SendSuccessEmail(bool diaria)
        {
            MailMessage mail = new MailMessage();
            string SMTP = _config.GetValue<string>("Mail:smtp");
            string port = _config.GetValue<string>("Mail:port");
            string user = _config.GetValue<string>("Mail:user");
            string password = _config.GetValue<string>("Mail:password");

            mail.From = new MailAddress(_config.GetValue<string>("Mail:mailFrom"));
            string emails = _config.GetValue<string>("Mail:mailTo");
            string[] emailList = emails.Split(',');

            // Agregar cada correo al destinatario
            foreach (string email in emailList)
            {
                mail.To.Add(email.Trim());
            }

            mail.Subject = $"Estado worker puntos de clientes Tienda {_config.GetValue<string>("Tienda:Nombre")}";

            if (!diaria)
                mail.Body = $"Se han sincronizado los puntos, tarea cada {_config.GetValue<int>("EjecutionTime:Instant")} minutos.";
            else
                mail.Body = $"Se han sincronizado los puntos, tarea diaria.";
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.Normal;
            var credentials = new NetworkCredential(user, password);
            SmtpClient smtp = new SmtpClient(SMTP, Convert.ToInt32(port));
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = credentials;
            smtp.Host = SMTP;
            smtp.Port = Convert.ToInt32(port);
            smtp.Send(mail);
        }

        private void Log(string mensaje)
        {
            StreamWriter sw = null;
            try
            {
                string filePath = Path.Combine(_config.GetValue<string>("Log:Folder"), "LogWorker.txt");

                //Preparar el mensaje
                var strBuilder = new StringBuilder();
                strBuilder.Append(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " - ");
                strBuilder.Append(mensaje);

                sw = new StreamWriter(filePath, true, Encoding.UTF8);
                sw.WriteLine(strBuilder.ToString());
                sw.Close();
            }
            catch (Exception)
            {
                //throw ex;
                throw new Exception("Ocurrió un error al generar el log");
            }
            finally
            {
                if (sw != null) sw.Dispose();
            }
        }

        private void LogError(string mensaje)
        {
            StreamWriter sw = null;
            try
            {
                string filePath = Path.Combine(_config.GetValue<string>("Log:Folder"), "LogWorkerError.txt");

                //Preparar el mensaje
                var strBuilder = new StringBuilder();
                strBuilder.Append(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " - ");
                strBuilder.Append(mensaje);

                sw = new StreamWriter(filePath, true, Encoding.UTF8);
                sw.WriteLine(strBuilder.ToString());
                sw.Close();
            }
            catch (Exception)
            {
                //throw ex;
                throw new Exception("Ocurrió un error al generar el log");
            }
            finally
            {
                if (sw != null) sw.Dispose();
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
}