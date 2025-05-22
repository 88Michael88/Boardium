using System.ComponentModel.DataAnnotations;

namespace Boardium.Models.Game;

public class GameImage
{
    public int Id { get; set; }
    [Required]
    [MaxLength(512)]
    public string ImagePath { get; set; } = string.Empty;
    [Required]
    public bool IsCoverImage { get; set; } = false;
    public int GameId { get; set; }
    public Game Game { get; set; }
}