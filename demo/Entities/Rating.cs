using catalog.Entities;

namespace catalog.Entities;

public class Rating
{
    public int Id { get; set; }
    public int Value { get; set; } // 1-5 звезди
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Връзки
    public int? MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int? BookId { get; set; }
    public Book? Book { get; set; }
}