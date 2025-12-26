using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timecrax.Api.Domain.Entities;

namespace Timecrax.Api.Data.Configurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        b.Property(x => x.Role)
            .HasColumnName("role")
            .HasColumnType("text")
            .IsRequired();

        b.Property(x => x.FirstName)
            .HasColumnName("fisrt_name")
            .HasColumnType("text")
            .IsRequired();

        b.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasColumnType("text")
            .IsRequired();

        b.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("citext")
            .IsRequired();

        b.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("uq_users_email");

        b.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("text")
            .IsRequired();

        b.Property(x => x.SchoolName)
            .HasColumnName("school_name")
            .HasColumnType("text");

        b.Property(x => x.Picture)
            .HasColumnName("picture")
            .HasMaxLength(500);

        b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        // checks (equivalentes ao seu SQL)
        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_users_role", "role IN ('student','teacher')");
            t.HasCheckConstraint("ck_users_full_name_len", "char_length(trim(full_name)) >= 2");
            t.HasCheckConstraint("ck_teacher_requires_school",
                "role <> 'teacher' OR (school_name IS NOT NULL AND char_length(trim(school_name)) > 0)");
        });
    }
}
