using Microsoft.EntityFrameworkCore;
using System.Linq;
using WorkerSubwayPruebas.Data;
using WorkerSubwayPruebas.Models;
using WorkerSubwayPruebas.Repository.IRepository;

namespace WorkerSubwayPruebas.Repository
{
    public class ClienteRepository : IClienteRepository
    {
        ApplicationDbContext _db;

        public ClienteRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public ICollection<cli_clientes> GetClientes()
        {
            return _db.cli_clientes
                .Where(c => c.CLI_CEDULA == "114770929")
                .Select(c => new cli_clientes
                {
                    CLI_IDENTIFICACION = c.CLI_IDENTIFICACION,
                    CLI_NOMBRE = c.CLI_NOMBRE ?? string.Empty,
                    CLI_CEDULA = c.CLI_CEDULA ?? string.Empty,
                    CLI_PROCESADO = c.CLI_PROCESADO ?? string.Empty,
                    CLI_CATEGORIACLIENTE = c.CLI_CATEGORIACLIENTE ?? string.Empty,
                    CLI_CODIGOENCLIENTE = c.CLI_CODIGOENCLIENTE ?? string.Empty,
                    CLI_TELEFONOCELULAR = c.CLI_TELEFONOCELULAR ?? string.Empty,
                    CLI_FAX = c.CLI_FAX ?? string.Empty,
                    CLI_EMAIL = c.CLI_EMAIL ?? string.Empty,
                    CLI_CODIGOPOSTAL = c.CLI_CODIGOPOSTAL ?? string.Empty,
                    CLI_PUNTOSDISPONIBLES = c.CLI_PUNTOSDISPONIBLES,
                    CREATEUSERID = c.CREATEUSERID ?? string.Empty
                })
                .Take(100)
                .ToList();
        }

        public string GetUltimoCliente()
        {
            var cia = string.Empty;

            cia = _db.Clientes
             .OrderByDescending(c => c.Cia)
             .Select(c => c.Cia)
             .FirstOrDefault();
            return cia;
        }

        public bool InsertCisCliente(clientes cliente)
        {
            _db.Clientes.Add(cliente);
            return _db.SaveChanges() > 0 ? true : false;
        }

        public void UpdateCliProcesado(cli_clientes cliente)
        {
            var existingCliente = _db.cli_clientes.Find(cliente.CLI_IDENTIFICACION);
            if (existingCliente != null)
            {
                existingCliente.CLI_PROCESADO = cliente.CLI_PROCESADO;
                _db.SaveChanges();
            }
        }

        public clientes GetCisClienteByCedula(string Cedula)
        {
            return _db.Clientes.Where(x => x.Cedula == Cedula).FirstOrDefault();
        }

        public void UpdateSaldoCisCliente(string cedula, decimal PuntosAsignar)
        {
            var existingClienteCis = _db.Clientes.FirstOrDefault(c => c.Cedula == cedula);
            existingClienteCis.Saldo = float.Parse(PuntosAsignar.ToString());
            _db.SaveChanges();
        }
    }
}
