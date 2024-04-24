namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pasword : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Utenti", "Password", c => c.String(nullable: false, maxLength: 100, fixedLength: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Utenti", "Password", c => c.String(maxLength: 100, fixedLength: true));
        }
    }
}
