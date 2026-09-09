using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTrackerMobile.Persistence.Migrations;

public partial class AddSwimmingActivityFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "PoolLengthMetres", table: "ActivityRecords", type: "INTEGER", nullable: true);
        migrationBuilder.AddColumn<int>(name: "PoolLengths", table: "ActivityRecords", type: "INTEGER", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PoolLengthMetres", table: "ActivityRecords");
        migrationBuilder.DropColumn(name: "PoolLengths", table: "ActivityRecords");
    }
}
