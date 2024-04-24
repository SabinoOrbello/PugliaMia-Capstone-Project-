namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recuperoPass : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Utenti", "TokenRecuperoPassword", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Utenti", "TokenRecuperoPassword");
        }
    }
}
