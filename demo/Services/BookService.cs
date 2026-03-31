using Microsoft.EntityFrameworkCore;
using catalog.Data;
using catalog.Entities;

namespace catalog.Services;

public class BookService : IBookService
{
    private readonly ApplicationDbContext _context;

    public BookService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Book> GetAll()
    {
        return _context.Books
            .Include(b => b.Ratings)
            .ThenInclude(r => r.User)
            .ToList();
    }

    public Book? GetById(int id)
    {
        return _context.Books
            .Include(b => b.Ratings)
            .ThenInclude(r => r.User)
            .FirstOrDefault(b => b.Id == id);
    }

    public void Add(Book book)
    {
        book.CreatedAt = DateTime.Now;
        _context.Books.Add(book);
        _context.SaveChanges();
    }

    public void Update(Book book)
    {
        _context.Books.Update(book);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var book = _context.Books.Find(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            _context.SaveChanges();
        }
    }
}