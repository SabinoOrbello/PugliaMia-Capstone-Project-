using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace PugliaMia.Models
{
    public partial class ModelDbContext : DbContext
    {
        public ModelDbContext()
            : base("name=ModelDbContext")
        {
        }

        public virtual DbSet<Carrello> Carrello { get; set; }
        public virtual DbSet<Categorie> Categorie { get; set; }
        public virtual DbSet<DettagliOrdine> DettagliOrdine { get; set; }
        public virtual DbSet<Ordini> Ordini { get; set; }
        public virtual DbSet<Pagamenti> Pagamenti { get; set; }
        public virtual DbSet<Prodotti> Prodotti { get; set; }
        public virtual DbSet<Recensioni> Recensioni { get; set; }
        public virtual DbSet<Roles> Role { get; set; }
        public virtual DbSet<Spedizioni> Spedizioni { get; set; }
        public virtual DbSet<StatiOrdine> StatiOrdine { get; set; }
        public virtual DbSet<Utenti> Utenti { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DettagliOrdine>()
                .Property(e => e.Prezzo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Ordini>()
                .Property(e => e.Totale)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Pagamenti>()
                .Property(e => e.TotalePagato)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Prodotti>()
                .Property(e => e.Prezzo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Roles>()
               .HasMany(e => e.Utenti)
               .WithMany(e => e.Roles)
               .Map(m => m.ToTable("UtentiRuolo").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<Utenti>()
              .Property(e => e.Nome)
              .IsFixedLength();

            modelBuilder.Entity<Utenti>()
                .Property(e => e.Password)
                .IsFixedLength();

            modelBuilder.Entity<Utenti>()
                .Property(e => e.Email)
                .IsFixedLength();

            modelBuilder.Entity<Utenti>()
                .Property(e => e.Role)
                .IsFixedLength();

            modelBuilder.Entity<Utenti>()
                .HasMany(e => e.Ordini)
                .WithRequired(e => e.Utenti)
                .WillCascadeOnDelete(false);
        }
    }
}
