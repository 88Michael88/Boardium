using System.ComponentModel.DataAnnotations;

namespace Boardium.Models.Game;

public class Publisher
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(200)]
    public string? Website { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();
}