using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    // المدير وموظف الاستقبال يقدروا يديروا بيانات الأطباء
    [Authorize(Roles = Roles.Admin + "," + Roles.Receptionist)]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Doctors?search=...
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Doctors.Include(d => d.Department).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.FullName.Contains(search) ||
                    d.Specialization.Contains(search));
            }

            ViewData["Search"] = search;

            var doctors = await query.OrderBy(d => d.FullName).ToListAsync();
            return View(doctors);
        }

        // GET: /Doctors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // GET: /Doctors/Create
        public async Task<IActionResult> Create()
        {
            await LoadDepartments();
            return View();
        }

        // POST: /Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            ModelState.Remove(nameof(Doctor.Department));

            if (!ModelState.IsValid)
            {
                await LoadDepartments(doctor.DepartmentId);
                return View(doctor);
            }

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الطبيب بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Doctors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            await LoadDepartments(doctor.DepartmentId);
            return View(doctor);
        }

        // POST: /Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            if (id != doctor.Id) return NotFound();

            ModelState.Remove(nameof(Doctor.Department));

            if (!ModelState.IsValid)
            {
                await LoadDepartments(doctor.DepartmentId);
                return View(doctor);
            }

            var existing = await _context.Doctors.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = doctor.FullName;
            existing.Specialization = doctor.Specialization;
            existing.PhoneNumber = doctor.PhoneNumber;
            existing.Email = doctor.Email;
            existing.DepartmentId = doctor.DepartmentId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات الطبيب";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Doctors/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // POST: /Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor != null)
            {
                if (doctor.Appointments.Any())
                {
                    TempData["Error"] = "لا يمكن حذف الطبيب لوجود مواعيد مرتبطة به";
                    return RedirectToAction(nameof(Index));
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف الطبيب";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDepartments(int? selectedId = null)
        {
            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedId);
        }
    }
}
