using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using catalog.Data;
using catalog.Entities;

namespace Catalog.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        // Добавяне на тестови данни
        SeedTestData(context);

        return context;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Добавяне на тестов потребител
        var testUser = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashedpassword123",
            CreatedAt = DateTime.Now
        };

        // Добавяне на тестови филми
        var testMovies = new List<Movie>
        {
            new Movie
            {
                Id = 1,
                Title = "The Matrix",
                Director = "Wachowski",
                Year = 1999,
                Description = "A computer hacker learns about the true nature of reality.",
                CreatedAt = DateTime.Now
            },
            new Movie
            {
                Id = 2,
                Title = "Inception",
                Director = "Nolan",
                Year = 2010,
                Description = "A thief who steals corporate secrets through dream-sharing.",
                CreatedAt = DateTime.Now
            }
        };

        // Добавяне на тестови книги
        var testBooks = new List<Book>
        {
            new Book
            {
                Id = 1,
                Title = "1984",
                Author = "George Orwell",
                Year = 1949,
                Description = "A dystopian social science fiction novel.",
                CreatedAt = DateTime.Now
            },
            new Book
            {
                Id = 2,
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                Year = 1960,
                Description = "A novel about racial injustice.",
                CreatedAt = DateTime.Now
            }
        };

        context.Users.Add(testUser);
        context.Movies.AddRange(testMovies);
        context.Books.AddRange(testBooks);
        context.SaveChanges();
    }
}