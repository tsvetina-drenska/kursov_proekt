using catalog.Entities;

namespace catalog.Services;

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

        public void Delete(int id)
        {
            var movie = GetById(id);
            if (movie != null)
            {
                movies.Remove(movie);
            }
        }
    }