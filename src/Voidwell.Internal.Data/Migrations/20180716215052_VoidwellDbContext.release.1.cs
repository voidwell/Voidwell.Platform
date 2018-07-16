using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Voidwell.Internal.Data.Migrations
{
    public partial class VoidwellDbContextrelease1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_post",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    publish_date = table.Column<DateTime>(type: "timestamp", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custom_event",
                columns: table => new
                {
                    id = table.Column<int>(type: "int4", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    description = table.Column<string>(type: "text", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp", nullable: false),
                    game_id = table.Column<string>(type: "text", nullable: true),
                    is_private = table.Column<bool>(type: "bool", nullable: false),
                    map_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    score_configuration = table.Column<string>(type: "text", nullable: true),
                    server_id = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custom_event_team",
                columns: table => new
                {
                    custom_event_id = table.Column<int>(type: "int4", nullable: false),
                    team_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_event_team", x => new { x.custom_event_id, x.team_id });
                    table.ForeignKey(
                        name: "fk_custom_event_team_custom_event_custom_event_id",
                        column: x => x.custom_event_id,
                        principalTable: "custom_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_post");

            migrationBuilder.DropTable(
                name: "custom_event_team");

            migrationBuilder.DropTable(
                name: "custom_event");
        }
    }
}
