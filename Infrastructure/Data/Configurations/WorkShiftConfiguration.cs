using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MESS.Infrastructure.Data.Configurations;

public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
        builder.ToTable("WorkShifts");

        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ws => ws.Code)
            .HasMaxLength(50);

        builder.HasOne(ws => ws.Department)
            .WithMany(d => d.WorkShifts)
            .HasForeignKey(ws => ws.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ws => ws.DefaultConversation)
            .WithMany()
            .HasForeignKey(ws => ws.DefaultConversationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
