using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PugliaMia.Models
{
    public class ModificaOrdineViewModel
    {
        public Ordini Ordine { get; set; }
        public Spedizioni Spedizione { get; set; }
        public Pagamenti Pagamento { get; set; }
        public List<DettagliOrdine> DettagliOrdine { get; set; }
    }
}