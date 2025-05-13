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
        private readonly DatabaseContext _context;

        public UpgradeRequestController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: UpgradeRequest
        public async Task<IActionResult> Index()
        {
            return View(await _context.UpgradeRequests.ToListAsync());
        }

        // GET: UpgradeRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var upgradeRequest = await _context.UpgradeRequests
                .FirstOrDefaultAsync(m => m.UpgradeRequestId == id);
            if (upgradeRequest == null)
            {
                return NotFound();
            }

            return View(upgradeRequest);
        }

        // GET: UpgradeRequest/Create
        public IActionResult Create()
        {
            ViewBag.RequestingUserIdentifier = new SelectList(_context.Users.ToList(), "UserId", "UserId");
            return View();
        }

        // POST: UpgradeRequest/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UpgradeRequestId,RequestingUserIdentifier,RequestingUserDisplayName")] UpgradeRequest upgradeRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(upgradeRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RequestingUserIdentifier = new SelectList(_context.Users.ToList(), "UserId", "UserId", upgradeRequest.RequestingUserIdentifier);
            return View(upgradeRequest);
        }

        // GET: UpgradeRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var upgradeRequest = await _context.UpgradeRequests.FindAsync(id);
            if (upgradeRequest == null)
            {
                return NotFound();
            }
            return View(upgradeRequest);
        }

        // POST: UpgradeRequest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    _context.Update(upgradeRequest);
                    await _context.SaveChangesAsync();
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

        // GET: UpgradeRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var upgradeRequest = await _context.UpgradeRequests
                .FirstOrDefaultAsync(m => m.UpgradeRequestId == id);
            if (upgradeRequest == null)
            {
                return NotFound();
            }

            return View(upgradeRequest);
        }

        // POST: UpgradeRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var upgradeRequest = await _context.UpgradeRequests.FindAsync(id);
            if (upgradeRequest != null)
            {
                _context.UpgradeRequests.Remove(upgradeRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UpgradeRequestExists(int id)
        {
            return _context.UpgradeRequests.Any(e => e.UpgradeRequestId == id);
        }
    }
}
