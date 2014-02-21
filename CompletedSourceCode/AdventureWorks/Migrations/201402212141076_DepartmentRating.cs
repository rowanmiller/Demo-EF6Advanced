namespace AdventureWorks.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DepartmentRating : DbMigration
    {
        public override void Up()
        {
            AddColumn("HumanResources.Department", "Rating", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("HumanResources.Department", "Rating");
        }
    }
}
