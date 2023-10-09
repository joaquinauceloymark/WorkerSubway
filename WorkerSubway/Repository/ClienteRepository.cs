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
                    CREATEUSERID = c.CREATEUSERID ?? string.Empty
                })
                .Take(100)
                .ToList();
        }

        public string GetUltimoCliente()
        {
            var cia = string.Empty;
            try
            {
                cia = _db.Clientes
                 .OrderByDescending(c => c.Cia)
                 .Select(c => c.Cia)
                 .FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw;
            }
            return cia;
        }

        public bool InsertCisCliente(clientes cliente)
        {
            try
            {
                _db.Clientes.Add(cliente);
                return _db.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {

                throw;
            }
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
    }
}
