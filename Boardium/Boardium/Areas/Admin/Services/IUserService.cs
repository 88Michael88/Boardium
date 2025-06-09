using Boardium.Areas.Admin.Models;
using Boardium.Models.Auth;

namespace Boardium.Areas.Admin.Services;

public interface IUserService
{
    Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize);
    Task<ApplicationUser?> GetUserByIdAsync(string id);
    Task<EditUserViewModel?> GetEditUserViewModelAsync(string id);
    Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(EditUserViewModel model);
    Task<bool> DeleteUserAsync(string id);
}