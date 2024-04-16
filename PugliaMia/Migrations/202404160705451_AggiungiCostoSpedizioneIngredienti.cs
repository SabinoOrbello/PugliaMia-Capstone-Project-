namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiCostoSpedizioneIngredienti : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prodotti", "CostoSpedizione", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Prodotti", "Ingredienti", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prodotti", "Ingredienti");
            DropColumn("dbo.Prodotti", "CostoSpedizione");
        }
    }
}
