using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MESS.Infrastructure.Data.Configurations;

public class MessageReadConfiguration : IEntityTypeConfiguration<MessageRead>
{
    public void Configure(EntityTypeBuilder<MessageRead> builder)
    {
        builder.ToTable("Message_Reads");

        // Composite primary key
        builder.HasKey(mr => new { mr.MessageId, mr.UserId });

        builder.Property(mr => mr.ReadAt)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(mr => mr.Message)
            .WithMany(m => m.MessageReads)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.User)
            .WithMany(u => u.MessageReads)
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
