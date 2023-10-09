using System.ComponentModel.DataAnnotations;

namespace WorkerSubwayPruebas.Models
{
    public class cis_clienteDto
    {
        [Key]
        public string CLI_IDENTIFICACION { get; set; }
        public string CLI_NOMBRE { get; set; }
        public string CLI_CEDULA { get; set; }
        public string CLI_PROCESADO { get; set; }
        public string CLI_NUMTARJETAACTIVA { get; set; }
    }
}
