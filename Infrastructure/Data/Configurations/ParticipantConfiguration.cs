using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MESS.Infrastructure.Data.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");

        // Composite primary key
        builder.HasKey(p => new { p.ConversationId, p.UserId });

        // Index for fast lookup of a user's conversations
        builder.HasIndex(p => new { p.UserId, p.ConversationId });

        builder.Property(p => p.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.JoinedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(p => p.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Participations)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
