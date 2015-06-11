namespace SnooNotesAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LastUpdatedRoles", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "AccessToken", c => c.String());
            AddColumn("dbo.AspNetUsers", "TokenExpires", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "TokenExpires");
            DropColumn("dbo.AspNetUsers", "AccessToken");
            DropColumn("dbo.AspNetUsers", "LastUpdatedRoles");
        }
    }
}
