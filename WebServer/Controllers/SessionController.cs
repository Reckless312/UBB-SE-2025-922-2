using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Model.Authentication;
using ServerAPI.Data;

namespace WebServer.Controllers
{
    public class SessionController : Controller
    {
        private readonly DatabaseContext context;

        public SessionController(DatabaseContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await this.context.Sessions.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Session? session = await this.context.Sessions
                .FirstOrDefaultAsync(model => model.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(this.context.Users.ToList(), "UserId", "UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,UserId")] Session session)
        {
            if (ModelState.IsValid)
            {
                session.SessionId = Guid.NewGuid();
                this.context.Add(session);
                await this.context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserId = new SelectList(this.context.Users.ToList(), "UserId", "UserId", session.UserId);
            return View(session);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Session? session = await this.context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            return View(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("SessionId,UserId")] Session session)
        {
            if (id != session.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this.context.Update(session);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId))
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
            return View(session);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Session? session = await this.context.Sessions
                .FirstOrDefaultAsync(model => model.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            Session? session = await this.context.Sessions.FindAsync(id);
            if (session != null)
            {
                this.context.Sessions.Remove(session);
            }

            await this.context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(Guid id)
        {
            return this.context.Sessions.Any(existingSession => existingSession.SessionId == id);
        }
    }
}
