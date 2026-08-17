using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MESS.Infrastructure.Data.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("Message_Reactions");

        // Composite primary key
        builder.HasKey(mr => new { mr.MessageId, mr.UserId });

        builder.Property(mr => mr.EmojiCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(mr => mr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(mr => mr.Message)
            .WithMany(m => m.MessageReactions)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.User)
            .WithMany(u => u.MessageReactions)
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
