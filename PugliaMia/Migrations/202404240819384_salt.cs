namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class salt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Utenti", "Salt", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Utenti", "Salt");
        }
    }
}
