 using catalog.Entities;

namespace catalog.Services;

    public interface IMovieService
    {
        List<Movie> GetAll();
        Movie? GetById(int id);
        void Add(Movie movie);
        void Delete(int id);
    }


