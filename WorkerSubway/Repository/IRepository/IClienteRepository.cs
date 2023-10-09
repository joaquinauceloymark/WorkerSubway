using WorkerSubwayPruebas.Models;

namespace WorkerSubwayPruebas.Repository.IRepository
{
    public interface IClienteRepository
    {
        public ICollection<cli_clientes> GetClientes();
        string GetUltimoCliente();
        bool InsertCisCliente(clientes cliente);
        void UpdateCliProcesado(cli_clientes cliente);
    }
}
