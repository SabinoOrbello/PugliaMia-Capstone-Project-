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
    }
}