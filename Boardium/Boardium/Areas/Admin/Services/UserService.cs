using Boardium.Areas.Admin.Mappers;
using Boardium.Areas.Admin.Models;
using Boardium.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Areas.Admin.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserMapper _userMapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        UserMapper userMapper,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userMapper = userMapper;
        _logger = logger;
    }

    public async Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetPagedUsersAsync(int page, int pageSize)
    {
        var total = await _userManager.Users.CountAsync();
        var users = await _userManager.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, total);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<EditUserViewModel?> GetEditUserViewModelAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var model = _userMapper.ToViewModel(user);
        model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        model.SelectedRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        return model;
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return (false, "User not found");

        _userMapper.UpdateUser(model, user);
        user.UserName = user.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to update user {UserId}", user.Id);
            return (false, "Failed to update user.");
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existingRoles);

        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        return (true, null);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        await _userManager.DeleteAsync(user);
        return true;
    }
}
