namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Security;

    [Table("Utenti")]
    public partial class Utenti
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Utenti()
        {
            Carrello = new HashSet<Carrello>();
            Ordini = new HashSet<Ordini>();
            Recensioni = new HashSet<Recensioni>();
        }

        [Key]
        public int UserID { get; set; }

        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Inserisci un'email valida")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Il campo Password è obbligatorio")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La lunghezza della password deve essere compresa tra 6 e 100 caratteri")]
        public string Password { get; set; }

        public string Salt { get; set; }

        [StringLength(255)]
        public string IndirizzoSpedizione { get; set; }

        [StringLength(10)]
        public string Role { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Carrello> Carrello { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ordini> Ordini { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Recensioni> Recensioni { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Roles> Roles { get; set; }
        public string TokenRecuperoPassword { get; internal set; }
    }
}
