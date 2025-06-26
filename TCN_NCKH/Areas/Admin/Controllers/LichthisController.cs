using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LichthisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public LichthisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Lichthis
        public async Task<IActionResult> Index()
        {
            var nghienCuuKhoaHocContext = _context.Lichthis.Include(l => l.Dethi).Include(l => l.Lophoc);
            return View(await nghienCuuKhoaHocContext.ToListAsync());
        }

        // GET: Admin/Lichthis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichthi = await _context.Lichthis
                .Include(l => l.Dethi)
                .Include(l => l.Lophoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichthi == null)
            {
                return NotFound();
            }

            return View(lichthi);
        }

        // GET: Admin/Lichthis/Create
        public IActionResult Create()
        {
            ViewData["Dethiid"] = new SelectList(_context.Dethis, "Id", "Id");
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id");
            return View();
        }

        // POST: Admin/Lichthis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Dethiid,Lophocid,Ngaythi,Thoigian")] Lichthi lichthi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lichthi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Dethiid"] = new SelectList(_context.Dethis, "Id", "Id", lichthi.Dethiid);
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id", lichthi.Lophocid);
            return View(lichthi);
        }

        // GET: Admin/Lichthis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichthi = await _context.Lichthis.FindAsync(id);
            if (lichthi == null)
            {
                return NotFound();
            }
            ViewData["Dethiid"] = new SelectList(_context.Dethis, "Id", "Id", lichthi.Dethiid);
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id", lichthi.Lophocid);
            return View(lichthi);
        }

        // POST: Admin/Lichthis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Dethiid,Lophocid,Ngaythi,Thoigian")] Lichthi lichthi)
        {
            if (id != lichthi.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lichthi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LichthiExists(lichthi.Id))
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
            ViewData["Dethiid"] = new SelectList(_context.Dethis, "Id", "Id", lichthi.Dethiid);
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id", lichthi.Lophocid);
            return View(lichthi);
        }

        // GET: Admin/Lichthis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichthi = await _context.Lichthis
                .Include(l => l.Dethi)
                .Include(l => l.Lophoc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichthi == null)
            {
                return NotFound();
            }

            return View(lichthi);
        }

        // POST: Admin/Lichthis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichthi = await _context.Lichthis.FindAsync(id);
            if (lichthi != null)
            {
                _context.Lichthis.Remove(lichthi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LichthiExists(int id)
        {
            return _context.Lichthis.Any(e => e.Id == id);
        }
    }
}
