using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerSubwayPruebas.Models
{
    [Table("clientes", Schema = "cis")]
    public class clientes
    {
        [Key]
        public string Cia { get; set; }   
        public string Codigo { get; set; }

        public double? Renta { get; set; }

        
        public string Nombre { get; set; }

        public string Telefono { get; set; }

        
        public string Cedula { get; set; }

        public string Apartado { get; set; }

        public DateTime? F_ult_compra { get; set; }

        public string Fax { get; set; }

        
        public short Plazo { get; set; }

        public string C_contable { get; set; }

        public short? Dia_cobro { get; set; }

        public int? Exento { get; set; }

        public double? IV { get; set; }

        public double? Saldo { get; set; }

        public string Linea1 { get; set; }

        public double? Saldo_Ant { get; set; }

        public string Linea2 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Aplica { get; set; }

        public double? Adelantos { get; set; }

        public string email { get; set; }

        
        public double Limi_cred { get; set; }

        
        public string Zona { get; set; }

        
        public string Agente { get; set; }

        public string Contacto { get; set; }

        
        public string Moneda { get; set; }

        public string Direccion { get; set; }

        public short? Dia_Tramit { get; set; }

        public double? Descuento { get; set; }

        public short? NumPrecio { get; set; }

        public short? ExentoRenta { get; set; }

        public short? Credito { get; set; }

        public double? Comision { get; set; }
    }
}
