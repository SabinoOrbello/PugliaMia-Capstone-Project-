namespace PugliaMia.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AggiungiCampiPagamentiStripe : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pagamenti", "StripePaymentIntentId", c => c.String(maxLength: 100));
            AddColumn("dbo.Pagamenti", "StripePaymentMethodId", c => c.String(maxLength: 100));
            AddColumn("dbo.Pagamenti", "StripePaymentIntentClientSecret", c => c.String(maxLength: 100));
            AddColumn("dbo.Pagamenti", "StripePaymentStatus", c => c.String(maxLength: 50));
        }

        public override void Down()
        {
            DropColumn("dbo.Pagamenti", "StripePaymentIntentId");
            DropColumn("dbo.Pagamenti", "StripePaymentMethodId");
            DropColumn("dbo.Pagamenti", "StripePaymentIntentClientSecret");
            DropColumn("dbo.Pagamenti", "StripePaymentStatus");
        }
    }
}
