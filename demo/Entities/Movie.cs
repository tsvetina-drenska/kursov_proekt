using catalog.Entities;

namespace catalog.Entities;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Връзка с рейтингите
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    // Среден рейтинг (изчислява се на момента)
    public double AverageRating => Ratings.Any() ? Ratings.Average(r => r.Value) : 0;
}