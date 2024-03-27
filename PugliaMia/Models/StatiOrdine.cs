namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StatiOrdine")]
    public partial class StatiOrdine
    {
        [Key]
        public int StatoID { get; set; }

        [StringLength(100)]
        public string Descrizione { get; set; }
    }
}
