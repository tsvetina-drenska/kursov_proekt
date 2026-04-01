using catalog.Data;       
using catalog.Entities;  
using System.Collections.Generic;
using System.Linq;

namespace catalog.Repositories
{
    public class BookRepository
    {
        private readonly ApplicationDbContext _context;

        // Конструктор
        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Връща всички книги
        public List<Book> GetAll()
        {
            return _context.Books.ToList();
        }

        // Връща книга по id
        public Book GetById(int id)
        {
            return _context.Books.FirstOrDefault(b => b.Id == id);
        }

        // Добавя нова книга
        public void Add(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }

        // Обновява книга
        public void Update(Book book)
        {
            _context.Books.Update(book);
            _context.SaveChanges();
        }

        // Изтрива книга по id
        public void Delete(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
        }
    }
}
