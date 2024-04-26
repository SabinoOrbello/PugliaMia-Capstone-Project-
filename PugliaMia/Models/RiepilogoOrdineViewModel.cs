using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace PugliaMia.Models
{
    public class RiepilogoOrdineViewModel
    {
        public Ordini Ordine { get; set; }
        public Spedizioni Spedizione { get; set; }
        public Pagamenti Pagamento { get; set; }
        public List<DettagliOrdine> DettagliOrdine { get; set; }
        public decimal TotaleOrdine { get; set; } // Aggiungi questo campo per il totale dell'ordine
        public decimal CostoSpedizioneTotale { get; set; }
        public DateTime DataConsegnaPrevista { get; internal set; }
    }
}
