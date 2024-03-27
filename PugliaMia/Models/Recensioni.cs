namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Recensioni")]
    public partial class Recensioni
    {
        [Key]
        public int RecensioneID { get; set; }

        public int? ProdottoID { get; set; }

        public int? UserID { get; set; }

        public int? Punteggio { get; set; }

        public string Commento { get; set; }

        public DateTime? DataRecensione { get; set; }

        public virtual Prodotti Prodotti { get; set; }

        public virtual Utenti Utenti { get; set; }
    }
}
