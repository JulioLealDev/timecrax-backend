using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timecrax.Api.Domain.Entities;

namespace Timecrax.Api.Data.Configurations;

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        b.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        b.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("text")
            .IsRequired();

        b.HasIndex(x => x.TokenHash)
            .IsUnique();

        b.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        b.Property(x => x.ReplacedByTokenHash)
            .HasColumnName("replaced_by_token_hash")
            .HasColumnType("text");
    }
}
