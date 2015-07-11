using Orchard.Data.Migration;

namespace Orchard.Recipes {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            //SchemaBuilder.CreateTable("RecipeResultRecord", table => table
            //    .Column<int>("Id", c => c.PrimaryKey().Identity())
            //    .Column<string>("ExecutionId", c => c.WithLength(128).Unique().NotNull())
            //    .Column<bool>("IsCompleted", c => c.NotNull())
            //);

            SchemaBuilder.CreateTable("RecipeStepResultRecord", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("ExecutionId", c => c.WithLength(128).NotNull())
                .Column<string>("StepName", c => c.WithLength(256).NotNull())
                .Column<bool>("IsCompleted", c => c.NotNull())
                .Column<bool>("IsSuccessful", c => c.NotNull())
                .Column<string>("ErrorMessage", c => c.Unlimited().Nullable())
            );

            SchemaBuilder.AlterTable("RecipeStepResultRecord", table => table
                .CreateIndex("IDX_RecipeStepResultRecord_ExecutionId", "ExecutionId")
            );

            SchemaBuilder.AlterTable("RecipeStepResultRecord", table => table
                .CreateIndex("IDX_RecipeStepResultRecord_ExecutionId_StepName", "ExecutionId", "StepName")
            );

            return 1;
        }
    }
}