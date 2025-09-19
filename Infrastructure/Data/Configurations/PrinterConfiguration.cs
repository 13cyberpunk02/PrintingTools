using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintingTools.Domain.Entities;

namespace PrintingTools.Infrastructure.Data.Configurations;

public class PrinterConfiguration : IEntityTypeConfiguration<Printer>
{
    public void Configure(EntityTypeBuilder<Printer> builder)
    {
        builder.ToTable("printers");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Model)
            .HasColumnName("model")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Location)
            .HasColumnName("location")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.NetworkPath)
            .HasColumnName("network_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.IsColorSupported)
            .HasColumnName("is_color_supported")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(p => p.MaxPaperWidth)
            .HasColumnName("max_paper_width")
            .IsRequired();

        builder.Property(p => p.MaxPaperHeight)
            .HasColumnName("max_paper_height")
            .IsRequired();

        builder.Property(p => p.LastMaintenanceDate)
            .HasColumnName("last_maintenance_date");

        builder.Property(p => p.PagesPrinted)
            .HasColumnName("pages_printed")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.CurrentJobsInQueue)
            .HasColumnName("current_jobs_in_queue")
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();
        
        builder.Property(p => p.SupportedFormats)
            .HasColumnName("supported_formats")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .HasColumnType("jsonb");

        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("ix_printers_name");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_printers_status");

        builder.HasIndex(p => p.Type)
            .HasDatabaseName("ix_printers_type");

        builder.HasIndex(p => p.IsDefault)
            .HasDatabaseName("ix_printers_default");

        builder.HasIndex(p => p.NetworkPath)
            .HasDatabaseName("ix_printers_network_path");
        
        builder.HasMany<PrintJob>()
            .WithOne(j => j.Printer)
            .HasForeignKey(j => j.PrinterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}