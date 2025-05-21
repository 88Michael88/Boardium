using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Boardium.Areas.Admin.Models;

public class GameFormViewModel
{
    public int? Id {get; set;}
    [Required]
    public string Title {get; set;}
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
    [Display(Name = "Publisher")]
    public int PublisherId {get; set;}

    [Display(Name = "Categories")] public List<SelectListItem> PublisherList { get; set; } = new();
    public List<int> SelectedCategoryIds {get; set;} = new();
    public List<SelectListItem> AllCategories {get; set;} = new();
    public string FormTitle => Id == null ? "Create Game" : "Edit Game";
}