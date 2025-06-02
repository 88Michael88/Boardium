using System.ComponentModel.DataAnnotations;
using Boardium.Models.Inventory;

namespace Boardium.Models.Game;

public class Game
{
    [Key] public int Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    [MaxLength(1024)]
    public string? Description { get; set; }
    [Range(1, 100)]
    public int MinPlayers { get; set; }
    [Range(1, 100)]
    public int MaxPlayers { get; set; }
    [Range(1, 120)]
    public int MinAge { get; set; }
    [Range(1, 120)]
    public int? MaxAge { get; set; }
    public int PlayingTimeMinutes { get; set; }

    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; }

    public ICollection<GameCategory> Categories { get; set; } = new List<GameCategory>();
    public ICollection<GameCopy> GameCopies { get; set; } = new List<GameCopy>();
    public ICollection<GameImage> Images { get; set; } = new List<GameImage>();
}