using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voidwell.Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeBlogIntoPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The legacy "blog_post" table is updated in place to match the Voidwell.Blog schema.
            migrationBuilder.RenameColumn(
                name: "content",
                table: "blog_post",
                newName: "html_content");

            // Legacy posts only have rendered content, so seed the markdown column from it.
            migrationBuilder.AddColumn<string>(
                name: "markdown_content",
                table: "blog_post",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE blog_post SET markdown_content = html_content;");

            // Legacy ids are text and dates are timestamp without time zone; convert them to uuid / timestamptz.
            migrationBuilder.Sql("ALTER TABLE blog_post ALTER COLUMN id TYPE uuid USING id::uuid;");
            migrationBuilder.Sql("ALTER TABLE blog_post ALTER COLUMN publish_date TYPE timestamp with time zone USING publish_date AT TIME ZONE 'UTC';");

            migrationBuilder.CreateTable(
                name: "blog_post_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    normalized_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog_post_tag_maps",
                columns: table => new
                {
                    blog_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_post_tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post_tag_maps", x => new { x.blog_post_id, x.blog_post_tag_id });
                    table.ForeignKey(
                        name: "fk_blog_post_tag_maps_blog_post_tags_blog_post_tag_id",
                        column: x => x.blog_post_tag_id,
                        principalTable: "blog_post_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blog_post_tag_maps_blog_posts_blog_post_id",
                        column: x => x.blog_post_id,
                        principalTable: "blog_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_tag_maps_blog_post_tag_id",
                table: "blog_post_tag_maps",
                column: "blog_post_tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_tags_normalized_name",
                table: "blog_post_tags",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_post_tag_maps");

            migrationBuilder.DropTable(
                name: "blog_post_tags");

            migrationBuilder.Sql("ALTER TABLE blog_post ALTER COLUMN publish_date TYPE timestamp without time zone USING publish_date AT TIME ZONE 'UTC';");
            migrationBuilder.Sql("ALTER TABLE blog_post ALTER COLUMN id TYPE text USING id::text;");

            migrationBuilder.DropColumn(
                name: "markdown_content",
                table: "blog_post");

            migrationBuilder.RenameColumn(
                name: "html_content",
                table: "blog_post",
                newName: "content");
        }
    }
}
