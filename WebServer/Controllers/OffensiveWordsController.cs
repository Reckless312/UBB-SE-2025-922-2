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
        private readonly DatabaseContext context;

        public OffensiveWordsController(DatabaseContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await this.context.OffensiveWords.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            OffensiveWord? offensiveWord = await this.context.OffensiveWords
                .FirstOrDefaultAsync(model => model.OffensiveWordId == id);
            if (offensiveWord == null)
            {
                return NotFound();
            }

            return View(offensiveWord);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OffensiveWordId,Word")] OffensiveWord offensiveWord)
        {
            if (ModelState.IsValid)
            {
                this.context.Add(offensiveWord);
                await this.context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(offensiveWord);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            OffensiveWord? offensiveWord = await this.context.OffensiveWords.FindAsync(id);
            if (offensiveWord == null)
            {
                return NotFound();
            }
            return View(offensiveWord);
        }

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
                    this.context.Update(offensiveWord);
                    await this.context.SaveChangesAsync();
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            OffensiveWord? offensiveWord = await this.context.OffensiveWords
                .FirstOrDefaultAsync(model => model.OffensiveWordId == id);
            if (offensiveWord == null)
            {
                return NotFound();
            }

            return View(offensiveWord);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            OffensiveWord? offensiveWord = await this.context.OffensiveWords.FindAsync(id);
            if (offensiveWord != null)
            {
                this.context.OffensiveWords.Remove(offensiveWord);
            }

            await this.context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OffensiveWordExists(int id)
        {
            return this.context.OffensiveWords.Any(existingOffensiveWord => existingOffensiveWord.OffensiveWordId == id);
        }
    }
}
