using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.DataConfigurations;

public class BlogPostTagMapConfiguration : IEntityTypeConfiguration<BlogPostTagMap>
{
    public void Configure(EntityTypeBuilder<BlogPostTagMap> builder)
    {
        builder.HasKey(a => new { a.BlogPostId, a.BlogPostTagId });

        builder.HasOne(a => a.BlogPost)
            .WithMany(a => a.BlogPostTagMaps)
            .HasForeignKey(a => a.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
