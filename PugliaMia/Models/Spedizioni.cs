namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Spedizioni")]
    public partial class Spedizioni
    {
        [Key]
        public int SpedizioneID { get; set; }

        public int? OrdineID { get; set; }

        [StringLength(255)]
        public string IndirizzoSpedizione { get; set; }
        public string Citta { get; set; }
        public string Regione { get; set; }
        public string Provincia { get; set; }

        [StringLength(100)]
        public string Corriere { get; set; }

        public DateTime? DataSpedizione { get; set; }

        [StringLength(100)]
        public string NumeroTracciamento { get; set; }

        [StringLength(50)]
        public string StatoSpedizione { get; set; }

        public virtual Ordini Ordini { get; set; }
    }
}
