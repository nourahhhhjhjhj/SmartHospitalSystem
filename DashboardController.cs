using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize] // يتطلب تسجيل دخول لأي دور كان
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

            var vm = new DashboardViewModel
            {
                FullName = user?.FullName ?? "مستخدم",
                Role = roles.FirstOrDefault() ?? "غير محدد",
                PatientsCount = await _context.Patients.CountAsync(),
                DoctorsCount = await _context.Doctors.CountAsync(),
                DepartmentsCount = await _context.Departments.CountAsync(),
                TodayAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == DateTime.Today),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.AppointmentDate >= DateTime.Now && a.Status == AppointmentStatus.Scheduled)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }

    public class DashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int PatientsCount { get; set; }
        public int DoctorsCount { get; set; }
        public int DepartmentsCount { get; set; }
        public int TodayAppointmentsCount { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; } = new();
    }
}
