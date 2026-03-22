using System.ComponentModel.DataAnnotations;

namespace catalog.Models;

public class BookViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Заглавието е задължително")]
    [StringLength(200, ErrorMessage = "Заглавието не може да надвишава 200 символа")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Авторът е задължителен")]
    [StringLength(100, ErrorMessage = "Името на автора не може да надвишава 100 символа")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Годината е задължителна")]
    [Range(1000, 2026, ErrorMessage = "Годината трябва да е между 1000 и 2026")]
    public int Year { get; set; }

    [StringLength(1000, ErrorMessage = "Описанието не може да надвишава 1000 символа")]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    // За показване в Views
    public double AverageRating { get; set; }
    public int RatingsCount { get; set; }
}