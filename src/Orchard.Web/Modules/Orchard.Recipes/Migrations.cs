using Orchard.Data.Migration;

namespace Orchard.Recipes {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("RecipeStepResultRecord", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("ExecutionId", c => c.WithLength(128).NotNull())
                .Column<string>("RecipeName", c => c.WithLength(256))
                .Column<string>("StepId", c => c.WithLength(32).NotNull())
                .Column<string>("StepName", c => c.WithLength(256).NotNull())
                .Column<bool>("IsCompleted", c => c.NotNull())
                .Column<bool>("IsSuccessful", c => c.NotNull())
                .Column<string>("ErrorMessage", c => c.Unlimited())
            );

            SchemaBuilder.AlterTable("RecipeStepResultRecord", table => {
                table.CreateIndex("IDX_RecipeStepResultRecord_ExecutionId", "ExecutionId");
                table.CreateIndex("IDX_RecipeStepResultRecord_ExecutionId_StepName", "ExecutionId", "StepName");
            });
            
            return 1;
        }
    }
}