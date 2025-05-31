using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Thrift.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<IdentityUser>
    {
        public void Configure(EntityTypeBuilder<IdentityUser> builder)
        {
            builder.Property(e => e.Id).HasMaxLength(85);
        }
    }

    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.Property(e => e.Id).HasMaxLength(85);
            builder.Property(e => e.Name).HasMaxLength(256);
            builder.Property(e => e.NormalizedName).HasMaxLength(256);
        }
    }

    public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
        {
            builder.Property(e => e.UserId).HasMaxLength(85);
        }
    }

    public class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
        {
            builder.Property(e => e.RoleId).HasMaxLength(85);
        }
    }

    public class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.Property(e => e.UserId).HasMaxLength(85);
            builder.Property(e => e.RoleId).HasMaxLength(85);
        }
    }

    public class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
        {
            builder.Property(e => e.LoginProvider).HasMaxLength(85);
            builder.Property(e => e.ProviderKey).HasMaxLength(85);
            builder.Property(e => e.UserId).HasMaxLength(85);
        }
    }

    public class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
        {
            builder.Property(e => e.UserId).HasMaxLength(85);
            builder.Property(e => e.LoginProvider).HasMaxLength(85);
            builder.Property(e => e.Name).HasMaxLength(85);
        }
    }
}