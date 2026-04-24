using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pro_exam.DataBaseContext;
using pro_exam.Models;

namespace pro_exam.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DoctorController : Controller
    {
        private readonly AppDBcontext _context;

        public DoctorController(AppDBcontext context)
        {
            _context = context;
        }

        public IActionResult AddNewDoctor()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddToDB(Doctor doctor)
        {
            var existingDoctor = _context.Doctors.FirstOrDefault(d => d.DoctorName == doctor.DoctorName);
            if (existingDoctor != null)
            {
                TempData["ErrorMessage"] = "Doctor with the same name already exists!";
                return RedirectToAction("AddNewDoctor");
            }

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Doctor has been added successfully!";
            return RedirectToAction("AddNewDoctor");
        }
    }
}
