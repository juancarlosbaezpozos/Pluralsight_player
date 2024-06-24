using System.Data;
using Dapper;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class CreateModuleAndClipTables : IMigration
{
	public int VersionNumber => 4;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection cnn = connectionManager.OpenConnection();
		cnn.Execute("CREATE TABLE Module ( Id Integer PRIMARY KEY,  Name text, Title text, AuthorHandle text, Description text, DurationInMilliseconds integer, ModuleIndex integer, CourseName text REFERENCES Course(Name) ON DELETE CASCADE);");
		cnn.Execute("CREATE TABLE Clip ( Id Integer PRIMARY KEY, Name text, Title text, ClipIndex integer, DurationInMilliseconds integer, SupportsStandard integer, SupportsWidescreen integer, ModuleId integer REFERENCES Module(Id) ON DELETE CASCADE);");
	}
}
