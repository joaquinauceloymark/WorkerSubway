using Microsoft.Data.SqlClient;
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


        public clientes GetCisClienteByCedula(string Cedula)
        {
            return _db.Clientes.Where(x => x.Codigo == Cedula).FirstOrDefault();
        }

        public void UpdateSaldoCisCliente(string cedula, decimal PuntosAsignar)
        {
            try
            {
                var parametros = new SqlParameter[]
                {
                    new SqlParameter("@Cedula", cedula),
                    new SqlParameter("@PuntosAsignar", (float)PuntosAsignar)
                };

                _db.Database.ExecuteSqlRaw("EXEC sp_actualizarPuntosAsignar @Cedula, @PuntosAsignar", parametros);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
