using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.DataConfigurations;

public class CustomEventConfiguration : IEntityTypeConfiguration<CustomEvent>
{
    public void Configure(EntityTypeBuilder<CustomEvent> builder)
    {
        builder.ToTable("custom_event");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).ValueGeneratedOnAdd();
    }
}
