using System.Collections.Generic;
using System.Linq;
using catalog.Data;
using catalog.Entities;

namespace catalog.Repositories;

public class MovieRepository
{
    private readonly ApplicationDbContext _context;

    // Конструктор
    public MovieRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Връща всички филми
    public List<Movie> GetAll()
    {
        return _context.Movies.ToList();
    }

    // Връща филм по id
    public Movie GetById(int id)
    {
        return _context.Movies.FirstOrDefault(m => m.Id == id);
    }

    // Добавя нов филм
    public void Add(Movie movie)
    {
        _context.Movies.Add(movie);
        _context.SaveChanges();
    }

    // Обновява филм
    public void Update(Movie movie)
    {
        _context.Movies.Update(movie);
        _context.SaveChanges();
    }

    // Изтрива филм по id
    public void Delete(int id)
    {
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            _context.SaveChanges();
        }
    }
}
