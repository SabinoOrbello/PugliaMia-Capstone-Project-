namespace PugliaMia.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Prodotti")]
    public partial class Prodotti
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Prodotti()
        {
            Carrello = new HashSet<Carrello>();
            DettagliOrdine = new HashSet<DettagliOrdine>();
            Recensioni = new HashSet<Recensioni>();
        }

        [Key]
        public int ProdottoID { get; set; }

        [StringLength(100)]
        public string Nome { get; set; }

        public string Descrizione { get; set; }

        public decimal? Prezzo { get; set; }

        [StringLength(255)]
        public string Immagine { get; set; }


        public int? CategoriaID { get; set; }

        public bool? Disponibilita { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Carrello> Carrello { get; set; }

        public virtual Categorie Categorie { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DettagliOrdine> DettagliOrdine { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Recensioni> Recensioni { get; set; }
    }
}
