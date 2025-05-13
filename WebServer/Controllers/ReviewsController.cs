using DataAccess.Model.AdminDashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Data;

namespace WebServer.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly DatabaseContext context;

        public ReviewsController(DatabaseContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await this.context.Reviews.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Review? review = await this.context.Reviews
                .FirstOrDefaultAsync(model => model.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(this.context.Users.ToList(), "UserId", "UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewId,UserId,Rating,Content,CreatedDate,NumberOfFlags,IsHidden")] Review review)
        {
            if (ModelState.IsValid)
            {
                this.context.Add(review);
                await this.context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserId = new SelectList(this.context.Users.ToList(), "UserId", "UserId", review.UserId);
            return View(review);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Review? review = await this.context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,UserId,Rating,Content,CreatedDate,NumberOfFlags,IsHidden")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this.context.Update(review);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId))
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
            return View(review);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Review? review = await this.context.Reviews
                .FirstOrDefaultAsync(model => model.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Review? review = await this.context.Reviews.FindAsync(id);
            if (review != null)
            {
                this.context.Reviews.Remove(review);
            }

            await this.context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return this.context.Reviews.Any(existingReview => existingReview.ReviewId == id);
        }
    }
}
