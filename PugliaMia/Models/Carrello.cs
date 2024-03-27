namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Carrello")]
    public partial class Carrello
    {
        public int CarrelloID { get; set; }

        public int? UserID { get; set; }

        public int? ProdottoID { get; set; }

        public int? Quantita { get; set; }

        public virtual Prodotti Prodotti { get; set; }

        public virtual Utenti Utenti { get; set; }
    }
}
