using catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace catalog.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Username)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(m => m.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(m => m.Director)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(m => m.Title);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(b => b.Author)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(b => b.Title);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Movie)
                .WithMany(m => m.Ratings)
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Book)
                .WithMany(b => b.Ratings)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}