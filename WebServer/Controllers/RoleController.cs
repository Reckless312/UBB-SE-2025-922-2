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
    public class RoleController : Controller
    {
        private readonly DatabaseContext context;

        public RoleController(DatabaseContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await this.context.Roles.ToListAsync());
        }

        public async Task<IActionResult> Details(RoleType id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Role? role = await this.context.Roles
                .FirstOrDefaultAsync(model => model.RoleType == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleType,RoleName")] Role role)
        {
            if (ModelState.IsValid)
            {
                this.context.Add(role);
                await this.context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        public async Task<IActionResult> Edit(RoleType id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Role? role = await this.context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleType id, [Bind("RoleType,RoleName")] Role role)
        {
            if (id != role.RoleType)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this.context.Update(role);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.RoleType))
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
            return View(role);
        }

        public async Task<IActionResult> Delete(RoleType id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Role? role = await context.Roles
                .FirstOrDefaultAsync(model => model.RoleType == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(RoleType id)
        {
            Role? role = await this.context.Roles.FindAsync(id);
            if (role != null)
            {
                this.context.Roles.Remove(role);
            }

            await this.context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoleExists(RoleType id)
        {
            return this.context.Roles.Any(existingRole => existingRole.RoleType == id);
        }
    }
}
