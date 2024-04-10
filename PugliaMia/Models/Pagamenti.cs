namespace PugliaMia.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Pagamenti")]
    public partial class Pagamenti
    {
        [Key]
        public int PagamentoID { get; set; }

        public int? OrdineID { get; set; }

        [StringLength(100)]
        public string MetodoPagamento { get; set; }

        public decimal? TotalePagato { get; set; }

        public DateTime? DataPagamento { get; set; }

        [StringLength(50)]
        public string StatoPagamento { get; set; }

        public virtual Ordini Ordini { get; set; }

        // Nuove proprietà per Stripe
        [StringLength(100)]
        public string StripePaymentIntentId { get; set; } // ID dell'intent di pagamento di Stripe

        [StringLength(100)]
        public string StripePaymentMethodId { get; set; } // ID del metodo di pagamento di Stripe

        [StringLength(100)]
        public string StripePaymentIntentClientSecret { get; set; } // Chiave segreta del client per l'intent di pagamento di Stripe

        [StringLength(50)]
        public string StripePaymentStatus { get; set; } // Stato del pagamento di Stripe


    }
}
