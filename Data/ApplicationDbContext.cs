using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thrift.Data.Configurations;
using Thrift.Models;

namespace Thrift.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Budget> Budgets { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; } // Added DbSet for UserProfile

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure your entity relationships and constraints here
            builder.Entity<Budget>()
                .HasKey(b => b.Id);

            builder.Entity<Budget>()
                .Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Budget>()
                .Property(b => b.UserId)
                .IsRequired();

            // Configure UserProfile
            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
