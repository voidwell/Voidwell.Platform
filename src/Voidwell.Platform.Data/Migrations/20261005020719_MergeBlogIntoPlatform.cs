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
            // The legacy "blog_post" table is intentionally left in place. Its schema is incompatible
            // with the Voidwell.Blog schema created below, so its data must be migrated separately.
            migrationBuilder.CreateTable(
                name: "blog_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    publish_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    markdown_content = table.Column<string>(type: "text", nullable: false),
                    html_content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_posts", x => x.id);
                });

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
                        principalTable: "blog_posts",
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

            migrationBuilder.DropTable(
                name: "blog_posts");
        }
    }
}
