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
        public float Renta { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Cedula { get; set; }
        public string Apartado { get; set; }
        public DateTime F_ult_compra { get; set; }
        public string Fax { get; set; }
        public int Plazo { get; set; }
        public string C_contable { get; set; }
        public int Dia_cobro { get; set; }
        public int Exento { get; set; }
        public float IV { get; set; }
        public float Saldo { get; set; }
        public string Linea1 { get; set; }
        public float Saldo_Ant { get; set; }
        public string Linea2 { get; set; }
        public float Aplica { get; set; }
        public float Adelantos { get; set; }
        public string email { get; set; }
        public float Limi_cred { get; set; }
        public string Zona { get; set; }
        public string Agente { get; set; }
        public string Contacto { get; set; }
        public string Moneda { get; set; }
        public string Direccion { get; set; }
        public int Dia_Tramit { get; set; }
        public float Descuento { get; set; }
        public int NumPrecio { get; set; }
        public int ExentoRenta { get; set; }
        public int Credito { get; set; }
        public float Comision { get; set; }
    }
}
