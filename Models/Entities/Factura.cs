﻿namespace Facturación.Models.Entities
{
    public class Factura
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public string Nombre { get; set; }
        public int TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public decimal Valor { get; set; }
        public decimal? TimeCompletion { get; set; }
    }
}
