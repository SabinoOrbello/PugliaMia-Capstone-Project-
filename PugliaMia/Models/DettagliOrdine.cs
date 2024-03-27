namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DettagliOrdine")]
    public partial class DettagliOrdine
    {
        [Key]
        public int DettaglioID { get; set; }

        public int? OrdineID { get; set; }

        public int? ProdottoID { get; set; }

        public int? Quantita { get; set; }

        public decimal? Prezzo { get; set; }

        public virtual Ordini Ordini { get; set; }

        public virtual Prodotti Prodotti { get; set; }
    }
}
