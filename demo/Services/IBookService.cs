 using catalog.Entities;

namespace catalog.Services;

    public interface IBookService
    {
        List<Book> GetAll();
        Book? GetById(int id);
        void Add(Book book);
        void Delete(int id);
    }
