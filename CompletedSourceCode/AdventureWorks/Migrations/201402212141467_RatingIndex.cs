namespace AdventureWorks.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RatingIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("HumanResources.Department", "Rating");
        }
        
        public override void Down()
        {
            DropIndex("HumanResources.Department", new[] { "Rating" });
        }
    }
}
