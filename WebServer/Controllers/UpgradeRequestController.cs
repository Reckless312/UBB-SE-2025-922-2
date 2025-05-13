using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Model.AdminDashboard;
using ServerAPI.Data;

namespace WebServer.Controllers
{
    public class UpgradeRequestController : Controller
    {
        private readonly DatabaseContext context;

        public UpgradeRequestController(DatabaseContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await this.context.UpgradeRequests.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            UpgradeRequest? upgradeRequest = await this.context.UpgradeRequests
                .FirstOrDefaultAsync(model => model.UpgradeRequestId == id);
            if (upgradeRequest == null)
            {
                return NotFound();
            }

            return View(upgradeRequest);
        }

        public IActionResult Create()
        {
            ViewBag.RequestingUserIdentifier = new SelectList(this.context.Users.ToList(), "UserId", "UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UpgradeRequestId,RequestingUserIdentifier,RequestingUserDisplayName")] UpgradeRequest upgradeRequest)
        {
            if (ModelState.IsValid)
            {
                this.context.Add(upgradeRequest);
                await this.context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RequestingUserIdentifier = new SelectList(this.context.Users.ToList(), "UserId", "UserId", upgradeRequest.RequestingUserIdentifier);
            return View(upgradeRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            UpgradeRequest? upgradeRequest = await this.context.UpgradeRequests.FindAsync(id);
            if (upgradeRequest == null)
            {
                return NotFound();
            }
            return View(upgradeRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UpgradeRequestId,RequestingUserIdentifier,RequestingUserDisplayName")] UpgradeRequest upgradeRequest)
        {
            if (id != upgradeRequest.UpgradeRequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this.context.Update(upgradeRequest);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UpgradeRequestExists(upgradeRequest.UpgradeRequestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(upgradeRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            UpgradeRequest? upgradeRequest = await this.context.UpgradeRequests
                .FirstOrDefaultAsync(model => model.UpgradeRequestId == id);
            if (upgradeRequest == null)
            {
                return NotFound();
            }

            return View(upgradeRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            UpgradeRequest? upgradeRequest = await this.context.UpgradeRequests.FindAsync(id);
            if (upgradeRequest != null)
            {
                this.context.UpgradeRequests.Remove(upgradeRequest);
            }

            await this.context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UpgradeRequestExists(int id)
        {
            return this.context.UpgradeRequests.Any(existingUpgradeRequest => existingUpgradeRequest.UpgradeRequestId == id);
        }
    }
}
