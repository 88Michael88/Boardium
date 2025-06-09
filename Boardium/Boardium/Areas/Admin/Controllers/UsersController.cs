using Boardium.Areas.Admin.Mappers;
using Boardium.Areas.Admin.Models;
using Boardium.Areas.Admin.Services;
using Boardium.Models.Auth;
using Boardium.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Areas.Admin.Controllers;


[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var (users, total) = await _userService.GetPagedUsersAsync(page, pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        return View(users);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var model = await _userService.GetEditUserViewModelAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = await _userService.GetEditUserViewModelAsync(model.Id) is { } vm ? vm.Roles : new List<string>();
            return View(model);
        }

        var (success, errorMessage) = await _userService.UpdateUserAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", errorMessage ?? "Unknown error.");
            model.Roles = await _userService.GetEditUserViewModelAsync(model.Id) is { } vm ? vm.Roles : new List<string>();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}