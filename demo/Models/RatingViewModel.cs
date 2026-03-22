using System.ComponentModel.DataAnnotations;

namespace catalog.Models;

public class RatingViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Името е задължително")]
    [StringLength(50, ErrorMessage = "Името не може да надвишава 50 символа")]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Оценката е задължителна")]
    [Range(1, 5, ErrorMessage = "Оценката трябва да е между 1 и 5")]
    public int Rating { get; set; }

    [StringLength(500, ErrorMessage = "Коментарът не може да надвишава 500 символа")]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    // За кой елемент е ревюто
    public int? MovieId { get; set; }
    public int? BookId { get; set; }
}