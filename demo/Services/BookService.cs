using catalog.Entities;

namespace catalog.Services;

    public class BookService : IBookService
    {
        private static List<Book> books = new List<Book>();

        public List<Book> GetAll()
        {
            return books;
        }

        public Book? GetById(int id)
        {
            return books.FirstOrDefault(b => b.Id == id);
        }

        public void Add(Book book)
        {
            book.Id = books.Count + 1;
            books.Add(book);
        }

        public void Delete(int id)
        {
            var book = GetById(id);
            if (book != null)
            {
                books.Remove(book);
            }
        }
    }