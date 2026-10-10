using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    // فقط المدير وموظف الاستقبال يديروا بيانات المرضى
    [Authorize(Roles = Roles.Admin + "," + Roles.Receptionist)]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Patients?search=...
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.FullName.Contains(search) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(search)));
            }

            ViewData["Search"] = search;

            var patients = await query.OrderByDescending(p => p.RegisteredAt).ToListAsync();
            return View(patients);
        }

        // GET: /Patients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();
            return View(patient);
        }

        // GET: /Patients/Create
        public IActionResult Create()
        {
            return View(new Patient { DateOfBirth = DateTime.Today.AddYears(-30) });
        }

        // POST: /Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (!ModelState.IsValid) return View(patient);

            patient.RegisteredAt = DateTime.UtcNow;
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل المريض بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Patients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: /Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id) return NotFound();
            if (!ModelState.IsValid) return View(patient);

            var existing = await _context.Patients.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = patient.FullName;
            existing.Gender = patient.Gender;
            existing.DateOfBirth = patient.DateOfBirth;
            existing.PhoneNumber = patient.PhoneNumber;
            existing.Address = patient.Address;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث بيانات المريض";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Patients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: /Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف المريض";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
