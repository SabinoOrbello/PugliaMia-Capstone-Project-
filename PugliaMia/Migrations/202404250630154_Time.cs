namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Time : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Ordini", "DataOrdine", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Ordini", "DataOrdine", c => c.DateTime());
        }
    }
}
