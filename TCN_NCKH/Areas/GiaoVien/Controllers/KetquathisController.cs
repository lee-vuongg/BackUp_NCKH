using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;

namespace TCN_NCKH.Areas.GiaoVien.Controllers
{
    [Area("GiaoVien")]
    public class KetquathisController : Controller
    {
        private readonly NghienCuuKhoaHocContext _context;

        public KetquathisController(NghienCuuKhoaHocContext context)
        {
            _context = context;
        }

        // GET: GiaoVien/Ketquathis
        public async Task<IActionResult> Index()
        {
            var nghienCuuKhoaHocContext = _context.Ketquathis.Include(k => k.Lichthi).Include(k => k.Sinhvien);
            return View(await nghienCuuKhoaHocContext.ToListAsync());
        }

        // GET: GiaoVien/Ketquathis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ketquathi = await _context.Ketquathis
                .Include(k => k.Lichthi)
                .Include(k => k.Sinhvien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ketquathi == null)
            {
                return NotFound();
            }

            return View(ketquathi);
        }

        // GET: GiaoVien/Ketquathis/Create
        public IActionResult Create()
        {
            ViewData["Lichthiid"] = new SelectList(_context.Lichthis, "Id", "Id");
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens, "Sinhvienid", "Sinhvienid");
            return View();
        }

        // POST: GiaoVien/Ketquathis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Sinhvienid,Lichthiid,Diem,Thoigianlam,Ngaythi")] Ketquathi ketquathi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ketquathi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Lichthiid"] = new SelectList(_context.Lichthis, "Id", "Id", ketquathi.Lichthiid);
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens, "Sinhvienid", "Sinhvienid", ketquathi.Sinhvienid);
            return View(ketquathi);
        }

        // GET: GiaoVien/Ketquathis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ketquathi = await _context.Ketquathis.FindAsync(id);
            if (ketquathi == null)
            {
                return NotFound();
            }
            ViewData["Lichthiid"] = new SelectList(_context.Lichthis, "Id", "Id", ketquathi.Lichthiid);
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens, "Sinhvienid", "Sinhvienid", ketquathi.Sinhvienid);
            return View(ketquathi);
        }

        // POST: GiaoVien/Ketquathis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Sinhvienid,Lichthiid,Diem,Thoigianlam,Ngaythi")] Ketquathi ketquathi)
        {
            if (id != ketquathi.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ketquathi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KetquathiExists(ketquathi.Id))
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
            ViewData["Lichthiid"] = new SelectList(_context.Lichthis, "Id", "Id", ketquathi.Lichthiid);
            ViewData["Sinhvienid"] = new SelectList(_context.Sinhviens, "Sinhvienid", "Sinhvienid", ketquathi.Sinhvienid);
            return View(ketquathi);
        }

        // GET: GiaoVien/Ketquathis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ketquathi = await _context.Ketquathis
                .Include(k => k.Lichthi)
                .Include(k => k.Sinhvien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ketquathi == null)
            {
                return NotFound();
            }

            return View(ketquathi);
        }

        // POST: GiaoVien/Ketquathis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketquathi = await _context.Ketquathis.FindAsync(id);
            if (ketquathi != null)
            {
                _context.Ketquathis.Remove(ketquathi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KetquathiExists(int id)
        {
            return _context.Ketquathis.Any(e => e.Id == id);
        }
    }
}
