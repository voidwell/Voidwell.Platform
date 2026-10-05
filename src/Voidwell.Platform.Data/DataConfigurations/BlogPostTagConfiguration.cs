using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.DataConfigurations;

public class BlogPostTagConfiguration : IEntityTypeConfiguration<BlogPostTag>
{
    public void Configure(EntityTypeBuilder<BlogPostTag> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.NormalizedName).IsUnique();
    }
}
