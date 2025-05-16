using System.ComponentModel.DataAnnotations;

namespace Boardium.Models.Game;

public class GameCategory
{
    public int Id { get; set; }
    [MaxLength(20)]
    [Required]
    public string Name { get; set; }

    public ICollection<Game> Games { get; set; } = new List<Game>();
}