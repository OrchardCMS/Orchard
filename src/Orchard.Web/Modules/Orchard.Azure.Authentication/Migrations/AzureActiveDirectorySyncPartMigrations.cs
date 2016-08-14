using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Extensions;
using Orchard.Azure.Authentication.Models;
using System;

namespace Orchard.Azure.Authentication.Migrations
{
	public class AzureActiveDirectorySyncPartMigrations : DataMigrationImpl
	{

		public int Create()
		{
			SchemaBuilder.CreateTable(typeof(AzureActiveDirectorySyncPartRecord).Name,
				table => table
					.ContentPartRecord() // .ContentPartRecord() will create an column named "Id", but will not add the Identity keyword.  
					//.Column<int>("Id", column => column.PrimaryKey().Identity())  //not required because this columen is created by .ContentPartRecord()
					.Column<int>("UserId", column => column.NotNull())
					.Column<DateTime>("LastSyncedUtc", column => column.NotNull())
			)
			.AlterTable(typeof(AzureActiveDirectorySyncPartRecord).Name,
				table => table
					.CreateIndex("UserId", "UserId") // Index the UserId column because that's how we'll nearly always query this table
			);

			return 1;
		}

		//public int UpdateFrom1()
		//{

		//	return 2;
		//}
	}
}