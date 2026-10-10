using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    // فقط المدير يدير الأقسام
    [Authorize(Roles = Roles.Admin)]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Doctors)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(departments);
        }

        // GET: /Departments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();
            return View(department);
        }

        // GET: /Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (!ModelState.IsValid) return View(department);

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة القسم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();
            return View(department);
        }

        // POST: /Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id) return NotFound();
            if (!ModelState.IsValid) return View(department);

            var existing = await _context.Departments.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = department.Name;
            existing.Description = department.Description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات القسم";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();
            return View(department);
        }

        // POST: /Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department != null)
            {
                if (department.Doctors.Any())
                {
                    TempData["Error"] = "لا يمكن حذف القسم لوجود أطباء مرتبطين به";
                    return RedirectToAction(nameof(Index));
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف القسم";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
