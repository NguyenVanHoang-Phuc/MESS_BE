using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MESS.Infrastructure.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .HasMaxLength(255);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.AvatarUrl)
            .HasMaxLength(1000);

        builder.Property(c => c.CanonicalKey)
            .HasMaxLength(100);

        builder.Property(c => c.IsSystemGroup)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(c => c.CanonicalKey)
            .IsUnique()
            .HasFilter("[CanonicalKey] IS NOT NULL");

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.WorkShift)
            .WithMany()
            .HasForeignKey(c => c.WorkShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Creator)
            .WithMany(u => u.CreatedConversations)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
