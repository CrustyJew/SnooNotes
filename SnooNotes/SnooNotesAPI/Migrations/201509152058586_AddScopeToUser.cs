namespace SnooNotesAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddScopeToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "HasWikiRead", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "HasRead", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "HasRead");
            DropColumn("dbo.AspNetUsers", "HasWikiRead");
        }
    }
}
