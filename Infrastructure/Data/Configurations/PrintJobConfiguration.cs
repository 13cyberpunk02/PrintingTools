using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintingTools.Domain.Entities;

namespace PrintingTools.Infrastructure.Data.Configurations;

public class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
{
    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder.ToTable("print_jobs");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.PrinterId)
            .HasColumnName("printer_id");

        builder.Property(p => p.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.FileSizeBytes)
            .HasColumnName("file_size_bytes")
            .IsRequired();

        builder.Property(p => p.FileType)
            .HasColumnName("file_type")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Format)
            .HasColumnName("format")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Copies)
            .HasColumnName("copies")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(p => p.IsColor)
            .HasColumnName("is_color")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(p => p.IsDuplex)
            .HasColumnName("is_duplex")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.Quality)
            .HasColumnName("quality")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Priority)
            .HasColumnName("priority")
            .HasDefaultValue(5)
            .IsRequired();

        builder.Property(p => p.TotalPages)
            .HasColumnName("total_pages");

        builder.Property(p => p.PrintedPages)
            .HasColumnName("printed_pages");

        builder.Property(p => p.QueuedAt)
            .HasColumnName("queued_at");

        builder.Property(p => p.StartedAt)
            .HasColumnName("started_at");

        builder.Property(p => p.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(p => p.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(p => p.SpoolJobId)
            .HasColumnName("spool_job_id")
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_print_jobs_user_id");

        builder.HasIndex(p => p.PrinterId)
            .HasDatabaseName("ix_print_jobs_printer_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_print_jobs_status");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("ix_print_jobs_created_at");

        builder.HasIndex(p => new { p.UserId, p.Status })
            .HasDatabaseName("ix_print_jobs_user_status");

        builder.HasIndex(p => new { p.Status, p.Priority, p.CreatedAt })
            .HasDatabaseName("ix_print_jobs_queue_order");

        builder.HasIndex(p => p.SpoolJobId)
            .HasDatabaseName("ix_print_jobs_spool_id");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Printer)
            .WithMany()
            .HasForeignKey(p => p.PrinterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}