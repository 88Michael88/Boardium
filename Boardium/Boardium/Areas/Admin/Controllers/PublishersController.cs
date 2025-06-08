using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boardium.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Boardium.Data;
using Boardium.Models.Game;
using Microsoft.AspNetCore.Authorization;

namespace Boardium.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class PublishersController : Controller
    {
        private readonly IPublishersService _publishersService;

        public PublishersController(IPublishersService publishersService)
        {
            _publishersService = publishersService ?? throw new ArgumentNullException(nameof(publishersService));
        }

        // GET: Admin/Publishers
        public async Task<IActionResult> Index()
        {
            var publishers = await _publishersService.GetAllAsync();
            return View(publishers);
        }

        // GET: Admin/Publishers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _publishersService.GetByIdAsync(id.Value);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // GET: Admin/Publishers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Publishers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Website")] Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                await _publishersService.CreateAsync(publisher);
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        // GET: Admin/Publishers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _publishersService.GetByIdAsync(id.Value);
            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }

        // POST: Admin/Publishers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Website")] Publisher publisher)
        {
            if (id != publisher.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(publisher);
            }

            try
            {
                var updateResult = await _publishersService.UpdateAsync(publisher);

                if (!updateResult)
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Publishers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _publishersService.GetByIdAsync(id.Value);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Admin/Publishers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _publishersService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

      
    }
}
