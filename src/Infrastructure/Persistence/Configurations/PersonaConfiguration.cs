using AiKocStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiKocStudio.Infrastructure.Persistence.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.ToneOfVoice).HasMaxLength(200);
        builder.Property(p => p.TargetAudience).HasMaxLength(500);
        builder.Property(p => p.Platform).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.DefaultAiProvider).HasMaxLength(50);
        builder.Property(p => p.SystemPromptTemplate).HasMaxLength(4000);
    }
}
