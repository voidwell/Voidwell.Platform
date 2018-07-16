using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data.DataConfigurations
{
    public class CustomEventTeamConfiguration : IEntityTypeConfiguration<CustomEventTeam>
    {
        public void Configure(EntityTypeBuilder<CustomEventTeam> builder)
        {
            builder.ToTable("CustomEventTeam");

            builder.HasKey(a => new { a.CustomEventId, a.TeamId });

            builder.HasOne(a => a.CustomEvent)
                .WithMany(a => a.Teams)
                .HasForeignKey(a => a.CustomEventId);
        }
    }
}
