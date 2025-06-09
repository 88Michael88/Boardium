using Boardium.Areas.DTOs.GameCopies;
using Boardium.Models.Inventory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Boardium.Areas.Admin.Services;

public interface IGameCopiesService
{
    Task<(IEnumerable<GameCopy> Items, int TotalPages)> GetGameCopiesAsync(int? gameId, int page, int pageSize);
    Task<GameCopy?> GetGameCopyDetailsAsync(int id);
    Task<GameCopy?> GetGameCopyByIdAsync(int id);
    Task<string> GenerateInventoryNumberAsync(int gameId);
    Task<bool> CreateGameCopyAsync(GameCopy gameCopy);
    Task<GameCopyResult> TryCreateGameCopyAsync(GameCopy gameCopy);
    Task<bool> UpdateGameCopyAsync(GameCopy gameCopy);
    Task<bool> DeleteGameCopyAsync(int id);
    Task<byte[]> GenerateBarcodeFromId(int gameCopyId);
    SelectList GetGameSelectList(int? selectedId = null);
    SelectList GetConditionSelectList(GameCondition? selectedCondition = null);
}