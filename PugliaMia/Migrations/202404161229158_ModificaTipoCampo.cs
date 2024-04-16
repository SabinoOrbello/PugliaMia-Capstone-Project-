namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ModificaTipoCampo : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Prodotti", "Peso", c => c.Decimal(nullable: true, precision: 18, scale: 2));
        }

        public override void Down()
        {
            AlterColumn("dbo.Prodotti", "Peso", c => c.String());
        }
    }
}
