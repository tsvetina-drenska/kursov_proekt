
using Microsoft.EntityFrameworkCore;
using catalog.Data;
using catalog.Entities;

namespace catalog.Services;

public class MovieService : IMovieService
{
    private readonly ApplicationDbContext _context;

    public MovieService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Movie> GetAll()
    {
        return _context.Movies
            .Include(m => m.Ratings)
            .ThenInclude(r => r.User)
            .ToList();
    }

    public Movie? GetById(int id)
    {
        return _context.Movies
            .Include(m => m.Ratings)
            .ThenInclude(r => r.User)
            .FirstOrDefault(m => m.Id == id);
    }

    public void Add(Movie movie)
    {
        movie.CreatedAt = DateTime.Now;
        _context.Movies.Add(movie);
        _context.SaveChanges();
    }

    public void Update(Movie movie)
    {
        _context.Movies.Update(movie);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var movie = _context.Movies.Find(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            _context.SaveChanges();
        }
    }
}
