using Boardium.Areas.Admin.Models;
using Boardium.Areas.DTOs.Games;
using Boardium.Models.Game;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Boardium.Areas.Admin.Services;

public interface IGamesService
{
    Task<List<SelectListItem>> GetAllCategoriesAsync();
    Task<List<SelectListItem>> GetAllPublishersAsync();
    Task<List<Game>> GetAllGamesAsync();
    Task<Game?> GetGameDetailsAsync(int id);
    Task<GameFormViewModel> PrepareGameFormViewModelAsync(int gameId);
    Task PopulateFormViewDataAsync(GameFormViewModel vm);
    Task<bool> SaveGameFormViewModel(GameFormViewModel vm);
    Task RemoveGameAsync(int gameId);
   
    Task<List<AutocompleteResult>> AutocompleteAsync(string term);
}