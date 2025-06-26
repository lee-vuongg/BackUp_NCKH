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
    public class SinhviensController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public SinhviensController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: Admin/Sinhviens
        public async Task<IActionResult> Index()
        {
            var nghienCuuKhoaHocContext = _context.Sinhviens.Include(s => s.Lophoc).Include(s => s.SinhvienNavigation);
            return View(await nghienCuuKhoaHocContext.ToListAsync());
        }

        // GET: Admin/Sinhviens/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = await _context.Sinhviens
                .Include(s => s.Lophoc)
                .Include(s => s.SinhvienNavigation)
                .FirstOrDefaultAsync(m => m.Sinhvienid == id);
            if (sinhvien == null)
            {
                return NotFound();
            }

            return View(sinhvien);
        }

        // GET: Admin/Sinhviens/Create
        public IActionResult Create()
        {
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id");
            ViewData["Sinhvienid"] = new SelectList(_context.Nguoidungs, "Id", "Id");
            return View();
        }

        // POST: Admin/Sinhviens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Sinhvienid,Msv,Lophocid")] Sinhvien sinhvien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sinhvien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id", sinhvien.Lophocid);
            ViewData["Sinhvienid"] = new SelectList(_context.Nguoidungs, "Id", "Id", sinhvien.Sinhvienid);
            return View(sinhvien);
        }

        // GET: Admin/Sinhviens/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = await _context.Sinhviens.FindAsync(id);
            if (sinhvien == null)
            {
                return NotFound();
            }
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id", sinhvien.Lophocid);
            ViewData["Sinhvienid"] = new SelectList(_context.Nguoidungs, "Id", "Id", sinhvien.Sinhvienid);
            return View(sinhvien);
        }

        // POST: Admin/Sinhviens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Sinhvienid,Msv,Lophocid")] Sinhvien sinhvien)
        {
            if (id != sinhvien.Sinhvienid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sinhvien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhvienExists(sinhvien.Sinhvienid))
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
            ViewData["Lophocid"] = new SelectList(_context.Lophocs, "Id", "Id", sinhvien.Lophocid);
            ViewData["Sinhvienid"] = new SelectList(_context.Nguoidungs, "Id", "Id", sinhvien.Sinhvienid);
            return View(sinhvien);
        }

        // GET: Admin/Sinhviens/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhvien = await _context.Sinhviens
                .Include(s => s.Lophoc)
                .Include(s => s.SinhvienNavigation)
                .FirstOrDefaultAsync(m => m.Sinhvienid == id);
            if (sinhvien == null)
            {
                return NotFound();
            }

            return View(sinhvien);
        }

        // POST: Admin/Sinhviens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sinhvien = await _context.Sinhviens.FindAsync(id);
            if (sinhvien != null)
            {
                _context.Sinhviens.Remove(sinhvien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SinhvienExists(string id)
        {
            return _context.Sinhviens.Any(e => e.Sinhvienid == id);
        }
    }
}
