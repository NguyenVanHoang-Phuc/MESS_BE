using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = MESS.Domain.Entities.Task;

namespace MESS.Infrastructure.Data.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Todo");

        builder.Property(t => t.RefType)
            .HasMaxLength(100);

        builder.Property(t => t.RefId)
            .HasMaxLength(100);

        // Index for background job reminder scanning: (Deadline, Status)
        builder.HasIndex(t => new { t.Deadline, t.Status });

        // One-to-one relationship with Message
        builder.HasOne(t => t.SourceMessage)
            .WithOne(m => m.Task)
            .HasForeignKey<Task>(t => t.SourceMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        // CreatedBy (mapped from AuditableEntity) acts as CreatorId
        builder.HasOne(t => t.Creator)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
