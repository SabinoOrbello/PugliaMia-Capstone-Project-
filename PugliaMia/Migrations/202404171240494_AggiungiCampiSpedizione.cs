namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiCampiSpedizione : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Spedizioni", "Citta", c => c.String());
            AddColumn("dbo.Spedizioni", "Regione", c => c.String());
            AddColumn("dbo.Spedizioni", "Provincia", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Spedizioni", "Provincia");
            DropColumn("dbo.Spedizioni", "Regione");
            DropColumn("dbo.Spedizioni", "Citta");
        }
    }
}
