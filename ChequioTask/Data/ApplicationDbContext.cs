using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChequioTask.Models;

namespace ChequioTask.Data
{
    // EF Core DbContext for Identity + application entities
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Application entities
        public DbSet<Cheque> Cheques => Set<Cheque>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // DB-level default for creation timestamp (UTC) on insert
            builder.Entity<Cheque>()
                   .Property(c => c.CreatedAtUtc)
                   .HasDefaultValueSql("GETUTCDATE()")
                   .ValueGeneratedOnAdd();
        }
    }
}

