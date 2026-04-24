using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using pro_exam.DataBaseContext;
using pro_exam.Models;
using pro_exam.ViewModel;

namespace pro_exam.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ExamController : Controller
    {
        private readonly AppDBcontext _context;

        public ExamController(AppDBcontext context)
        {
            _context = context;
        }

        public IActionResult ExamDashBoard()
        {
            var exams = _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Room)
                .Include(e => e.Monitorings).ThenInclude(m => m.Doctor)
                .Select(exam => new ExamWithAvailableDoctorsViewModel
                {
                    ExamId = exam.Id,
                    CourseName = exam.Course.CourseName,
                    ExamDate = exam.ExamDate,
                    StartTime = exam.StartTime,
                    EndTime = exam.EndTime,
                    RoomName = exam.Room.Name,
                    AssignedDoctors = exam.Monitorings.Select(m => m.Doctor.DoctorName).ToList()
                }).ToList();

            return View(exams);
        }

        public IActionResult AddExam()
        {
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddExam(Exam exam)
        {
            if (ModelState.IsValid)
            {
                _context.Exams.Add(exam);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Exam added successfully!";
                return RedirectToAction("ExamDashBoard");
            }
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View(exam);
        }

        public IActionResult EditExam(int id)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null) return NotFound();
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View(exam);
        }

        [HttpPost]
        public IActionResult SaveEditExam(Exam exam)
        {
            if (ModelState.IsValid)
            {
                _context.Exams.Update(exam);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Exam updated successfully!";
                return RedirectToAction("ExamDashBoard");
            }
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View(exam);
        }

        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.Find(id);
            if (exam == null) return NotFound();
            _context.Exams.Remove(exam);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Exam deleted successfully!";
            return RedirectToAction("ExamDashBoard");
        }

        [HttpPost]
        public IActionResult ResetDatabase()
        {
            try
            {
                _context.Exams.RemoveRange(_context.Exams);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Database has been reset successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }
            return RedirectToAction("ExamDashBoard");
        }

        [HttpGet]
        public IActionResult GenerateExamReport()
        {
            var exams = _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Room)
                .Include(e => e.Monitorings).ThenInclude(m => m.Doctor)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Exam Report");

            worksheet.Cells[1, 1].Value = "Exam ID";
            worksheet.Cells[1, 2].Value = "Course Name";
            worksheet.Cells[1, 3].Value = "Room";
            worksheet.Cells[1, 4].Value = "Exam Date";
            worksheet.Cells[1, 5].Value = "Start Time";
            worksheet.Cells[1, 6].Value = "End Time";
            worksheet.Cells[1, 7].Value = "Assigned Doctors";

            int row = 2;
            foreach (var exam in exams)
            {
                worksheet.Cells[row, 1].Value = exam.Id;
                worksheet.Cells[row, 2].Value = exam.Course?.CourseName;
                worksheet.Cells[row, 3].Value = exam.Room?.Name;
                worksheet.Cells[row, 4].Value = exam.ExamDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = exam.StartTime.ToString(@"hh\:mm");
                worksheet.Cells[row, 6].Value = exam.EndTime.ToString(@"hh\:mm");
                worksheet.Cells[row, 7].Value = string.Join(", ", exam.Monitorings.Select(m => m.Doctor.DoctorName));
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Exam_Report.xlsx");
        }
    }
}
