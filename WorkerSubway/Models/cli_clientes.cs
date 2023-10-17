using System.ComponentModel.DataAnnotations;

namespace WorkerSubwayPruebas.Models
{
    public class cli_clientes
    {   
        [Key]
        public decimal CLI_IDENTIFICACION { get; set; }
        public string? CLI_NOMBRE { get; set; }
        public string? CLI_CEDULA { get; set; }
        public string? CLI_PROCESADO { get; set; }
        public string? CLI_NUMTARJETAACTIVA { get; set; }
        public string? CLI_CATEGORIACLIENTE { get; set; }
        public string? CLI_CODIGOENCLIENTE { get; set; }
        public string? CLI_TELEFONOCELULAR { get; set; }
        public string? CLI_FAX { get; set; }
        public string? CLI_EMAIL { get; set; }
        public string? CLI_CODIGOPOSTAL { get; set; }
        public decimal CLI_PUNTOSDISPONIBLES { get; set; }
        public string? CREATEUSERID { get; set; }

        public DateTime CREATEDATE { get; set; }
    }

}
