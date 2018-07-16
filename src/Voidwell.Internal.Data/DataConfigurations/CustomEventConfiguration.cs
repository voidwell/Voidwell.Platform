using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data.DataConfigurations
{
    public class CustomEventConfiguration : IEntityTypeConfiguration<CustomEvent>
    {
        public void Configure(EntityTypeBuilder<CustomEvent> builder)
        {
            builder.ToTable("CustomEvent");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id).ValueGeneratedOnAdd();
        }
    }
}
