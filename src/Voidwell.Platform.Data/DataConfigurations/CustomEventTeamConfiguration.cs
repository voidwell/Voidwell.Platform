using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data.DataConfigurations;

public class CustomEventTeamConfiguration : IEntityTypeConfiguration<CustomEventTeam>
{
    public void Configure(EntityTypeBuilder<CustomEventTeam> builder)
    {
        builder.ToTable("custom_event_team");

        builder.HasKey(a => new { a.CustomEventId, a.TeamId });

        builder.HasOne(a => a.CustomEvent)
            .WithMany(a => a.Teams)
            .HasForeignKey(a => a.CustomEventId);
    }
}
