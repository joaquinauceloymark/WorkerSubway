using WorkerSubwayPruebas.Models;

namespace WorkerSubwayPruebas.Repository.IRepository
{
    public interface IClienteRepository
    {
        string GetUltimoCliente();
        bool InsertCisCliente(clientes cliente);
        clientes GetCisClienteByCedula(string cLI_IDENTIFICACION);
        void UpdateSaldoCisCliente(string cedula, decimal cLI_PUNTOSDISPONIBLES);
    }
}
