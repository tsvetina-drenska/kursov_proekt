
using catalog.Entities;

namespace catalog.Services
{
    public class MovieService : IMovieService
    {
        private static List<Movie> movies = new List<Movie>();

        public List<Movie> GetAll()
        {
            return movies;
        }

        public Movie? GetById(int id)
        {
            return movies.FirstOrDefault(m => m.Id == id);
        }

        public void Add(Movie movie)
        {
            movie.Id = movies.Count + 1;
            movies.Add(movie);
        }

        public void Update(Movie movie)
        {
            var existing = movies.FirstOrDefault(m => m.Id == movie.Id);

            if (existing != null)
            {
                existing.Title = movie.Title;
                existing.Director = movie.Director;
                existing.Year = movie.Year;
                existing.Description = movie.Description;
                existing.ImageUrl = movie.ImageUrl;
            }
        }

        public void Delete(int id)
        {
            var movie = GetById(id);
            if (movie != null)
            {
                movies.Remove(movie);
            }
        }
    }
}
