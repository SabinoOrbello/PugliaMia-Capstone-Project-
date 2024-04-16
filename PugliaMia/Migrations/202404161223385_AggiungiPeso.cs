namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiPeso : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prodotti", "Peso", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prodotti", "Peso");
        }
    }
}
