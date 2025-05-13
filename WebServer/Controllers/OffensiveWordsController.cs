using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Model.AutoChecker;
using ServerAPI.Data;

namespace WebServer.Controllers
{
    public class OffensiveWordsController : Controller
    {
        private readonly DatabaseContext _context;

        public OffensiveWordsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: OffensiveWords
        public async Task<IActionResult> Index()
        {
            return View(await _context.OffensiveWords.ToListAsync());
        }

        // GET: OffensiveWords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offensiveWord = await _context.OffensiveWords
                .FirstOrDefaultAsync(m => m.OffensiveWordId == id);
            if (offensiveWord == null)
            {
                return NotFound();
            }

            return View(offensiveWord);
        }

        // GET: OffensiveWords/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OffensiveWords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OffensiveWordId,Word")] OffensiveWord offensiveWord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(offensiveWord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(offensiveWord);
        }

        // GET: OffensiveWords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offensiveWord = await _context.OffensiveWords.FindAsync(id);
            if (offensiveWord == null)
            {
                return NotFound();
            }
            return View(offensiveWord);
        }

        // POST: OffensiveWords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OffensiveWordId,Word")] OffensiveWord offensiveWord)
        {
            if (id != offensiveWord.OffensiveWordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(offensiveWord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OffensiveWordExists(offensiveWord.OffensiveWordId))
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
            return View(offensiveWord);
        }

        // GET: OffensiveWords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offensiveWord = await _context.OffensiveWords
                .FirstOrDefaultAsync(m => m.OffensiveWordId == id);
            if (offensiveWord == null)
            {
                return NotFound();
            }

            return View(offensiveWord);
        }

        // POST: OffensiveWords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offensiveWord = await _context.OffensiveWords.FindAsync(id);
            if (offensiveWord != null)
            {
                _context.OffensiveWords.Remove(offensiveWord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OffensiveWordExists(int id)
        {
            return _context.OffensiveWords.Any(e => e.OffensiveWordId == id);
        }
    }
}
