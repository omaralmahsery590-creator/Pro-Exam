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
                .Select(exam => new ExamWithAvailableDoctorsViewModel
                {
                    ExamId = exam.Id,
                    CourseName = exam.Course.CourseName,
                    ExamDate = exam.ExamDate,
                    StartTime = exam.StartTime,
                    EndTime = exam.EndTime,
                    RoomName = exam.Room.Name,
                    ExtraRoomNames = exam.ExtraRooms.Select(r => r.Room.Name).ToList(),
                    AssignedDoctors = exam.Monitorings.Select(m => m.Doctor.DoctorName).ToList()
                }).ToList();

            return View(exams);
        }

        public IActionResult AddExam()
        {
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Doctors = _context.Doctors.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddExamAjax([FromBody] pro_exam.ViewModel.AddExamRequest request)
        {
            if (request == null)
                return Json(new { status = "error", message = "بيانات غير صالحة" });

            var course = _context.Courses.Find(request.CourseId);
            if (course == null)
                return Json(new { status = "error", message = "المادة غير موجودة" });

            var room = _context.Rooms.Find(request.RoomId);
            if (room == null)
                return Json(new { status = "error", message = "القاعة غير موجودة" });

            if (!TimeSpan.TryParse(request.ExamTime, out TimeSpan examTime))
                return Json(new { status = "error", message = "وقت الامتحان غير صحيح" });

            var examDate = request.ExamDate.Date;

            // Rule 1: Duplicate exam - same course cannot be scheduled more than once
            if (_context.Exams.Any(e => e.CourseId == request.CourseId))
                return Json(new { status = "conflict", message = "تكرار: تم جدولة امتحان لهذه المادة مسبقاً" });

            // Rule 2: Level conflict - max 2 exams per academic level per day
            int sameLevelCount = _context.Exams.Include(e => e.Course)
                .Count(e => e.ExamDate.Date == examDate && e.Course.Level == course.Level);
            if (sameLevelCount >= 2)
                return Json(new { status = "conflict", message = $"تعارض المستوى: وصل الحد الأقصى (2 امتحانات) لمستوى {course.Level} في يوم {examDate:yyyy-MM-dd}" });

            // Collect all rooms to check (primary + extra), deduped
            var extraRoomIds = request.ExtraRoomIds?.Where(id => id > 0 && id != request.RoomId).Distinct().ToList() ?? new List<int>();
            var allRoomIds = new List<int> { request.RoomId };
            allRoomIds.AddRange(extraRoomIds);

            // Rule 1: No two exams sharing a room (primary or extra) at the same date and time
            foreach (var roomId in allRoomIds)
            {
                var primaryRoomConflict = _context.Exams
                    .Any(e => e.RoomId == roomId && e.ExamDate.Date == examDate && e.StartTime == examTime);
                if (primaryRoomConflict)
                {
                    var conflictRoom = _context.Rooms.Find(roomId);
                    return Json(new { status = "conflict", message = "تعارض قاعة: القاعة \"" + (conflictRoom?.Name ?? "") + "\" محجوزة في نفس الوقت والتاريخ" });
                }

                var extraRoomConflict = _context.ExamExtraRooms
                    .Include(r => r.Exam)
                    .Any(r => r.RoomId == roomId && r.Exam.ExamDate.Date == examDate && r.Exam.StartTime == examTime);
                if (extraRoomConflict)
                {
                    var conflictRoom = _context.Rooms.Find(roomId);
                    return Json(new { status = "conflict", message = "تعارض قاعة: القاعة \"" + (conflictRoom?.Name ?? "") + "\" محجوزة كقاعة إضافية في نفس الوقت والتاريخ" });
                }
            }

            // Rule 2: No doctor (primary or extra) can proctor two exams at the same date and time
            var allDoctorIds = new List<int> { request.DoctorId };
            if (request.ExtraDoctorIds != null)
                allDoctorIds.AddRange(request.ExtraDoctorIds.Where(id => id > 0 && id != request.DoctorId));

            foreach (var doctorId in allDoctorIds)
            {
                var doctorConflict = _context.Monterings
                    .Include(m => m.Exam)
                    .Any(m => m.DoctorId == doctorId && m.Exam.ExamDate.Date == examDate && m.Exam.StartTime == examTime);
                if (doctorConflict)
                {
                    var conflictDoctor = _context.Doctors.Find(doctorId);
                    return Json(new { status = "conflict", message = "تعارض مراقب: الدكتور \"" + (conflictDoctor?.DoctorName ?? "") + "\" لديه امتحان آخر في نفس الوقت وهذا التاريخ" });
                }
            }

            var exam = new Exam
            {
                CourseId = request.CourseId,
                RoomId = request.RoomId,
                ExamDate = request.ExamDate,
                StartTime = examTime,
                EndTime = examTime.Add(TimeSpan.FromHours(2))
            };

            _context.Exams.Add(exam);
            _context.SaveChanges();

            foreach (var doctorId in allDoctorIds)
                _context.Monterings.Add(new Montering { DoctorId = doctorId, ExamId = exam.Id });

            foreach (var extraRoomId in extraRoomIds)
                _context.ExamExtraRooms.Add(new ExamExtraRoom { RoomId = extraRoomId, ExamId = exam.Id });

            _context.SaveChanges();

            var roomNames = room.Name;
            if (extraRoomIds.Count > 0)
            {
                var extraRoomNames = _context.Rooms.Where(r => extraRoomIds.Contains(r.Id)).Select(r => r.Name).ToList();
                roomNames += " + " + string.Join(" + ", extraRoomNames);
            }

            return Json(new { status = "success", message = "تم جدولة الامتحان بنجاح في: " + roomNames });
        }

        public IActionResult EditExam(int id)
        {
            var exam = _context.Exams
                .Include(e => e.Monitorings)
                .Include(e => e.ExtraRooms)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.Id == id);
            if (exam == null) return NotFound();
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Doctors = _context.Doctors.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            var sortedMonitorings = exam.Monitorings.OrderBy(m => m.Id).ToList();
            ViewBag.PrimaryDoctorId = sortedMonitorings.FirstOrDefault()?.DoctorId ?? 0;
            ViewBag.ExtraDoctorIds = sortedMonitorings.Skip(1).Select(m => m.DoctorId).ToList();
            ViewBag.ExtraRoomIds = exam.ExtraRooms.Select(r => r.RoomId).ToList();
            return View(exam);
        }

        [HttpPost]
        public IActionResult EditExamAjax([FromBody] EditExamRequest request)
        {
            if (request == null)
                return Json(new { status = "error", message = "بيانات غير صالحة" });

            var exam = _context.Exams
                .Include(e => e.Monitorings)
                .Include(e => e.ExtraRooms)
                .FirstOrDefault(e => e.Id == request.ExamId);
            if (exam == null)
                return Json(new { status = "error", message = "الامتحان غير موجود" });

            var course = _context.Courses.Find(request.CourseId);
            if (course == null)
                return Json(new { status = "error", message = "المادة غير موجودة" });

            var room = _context.Rooms.Find(request.RoomId);
            if (room == null)
                return Json(new { status = "error", message = "القاعة غير موجودة" });

            if (!TimeSpan.TryParse(request.ExamTime, out TimeSpan examTime))
                return Json(new { status = "error", message = "وقت الامتحان غير صحيح" });

            var examDate = request.ExamDate.Date;

            // Rule 1: Duplicate exam (exclude current exam)
            if (_context.Exams.Any(e => e.CourseId == request.CourseId && e.Id != request.ExamId))
                return Json(new { status = "conflict", message = "تكرار: تم جدولة امتحان لهذه المادة مسبقاً" });

            // Rule 2: Level conflict - max 2 exams per academic level per day (exclude current exam)
            int sameLevelCount = _context.Exams.Include(e => e.Course)
                .Count(e => e.ExamDate.Date == examDate && e.Course.Level == course.Level && e.Id != request.ExamId);
            if (sameLevelCount >= 2)
                return Json(new { status = "conflict", message = $"تعارض المستوى: وصل الحد الأقصى (2 امتحانات) لمستوى {course.Level} في يوم {examDate:yyyy-MM-dd}" });

            var extraRoomIds = request.ExtraRoomIds?.Where(id => id > 0 && id != request.RoomId).Distinct().ToList() ?? new List<int>();
            var allRoomIds = new List<int> { request.RoomId };
            allRoomIds.AddRange(extraRoomIds);

            // Rule 3: Room conflict (exclude current exam)
            foreach (var roomId in allRoomIds)
            {
                var primaryRoomConflict = _context.Exams
                    .Any(e => e.RoomId == roomId && e.ExamDate.Date == examDate && e.StartTime == examTime && e.Id != request.ExamId);
                if (primaryRoomConflict)
                {
                    var conflictRoom = _context.Rooms.Find(roomId);
                    return Json(new { status = "conflict", message = "تعارض قاعة: القاعة \"" + (conflictRoom?.Name ?? "") + "\" محجوزة في نفس الوقت والتاريخ" });
                }

                var extraRoomConflict = _context.ExamExtraRooms
                    .Include(r => r.Exam)
                    .Any(r => r.RoomId == roomId && r.Exam.ExamDate.Date == examDate && r.Exam.StartTime == examTime && r.ExamId != request.ExamId);
                if (extraRoomConflict)
                {
                    var conflictRoom = _context.Rooms.Find(roomId);
                    return Json(new { status = "conflict", message = "تعارض قاعة: القاعة \"" + (conflictRoom?.Name ?? "") + "\" محجوزة كقاعة إضافية في نفس الوقت والتاريخ" });
                }
            }

            // Rule 4: Doctor conflict (exclude current exam)
            var allDoctorIds = new List<int> { request.DoctorId };
            if (request.ExtraDoctorIds != null)
                allDoctorIds.AddRange(request.ExtraDoctorIds.Where(id => id > 0 && id != request.DoctorId));

            foreach (var doctorId in allDoctorIds)
            {
                var doctorConflict = _context.Monterings
                    .Include(m => m.Exam)
                    .Any(m => m.DoctorId == doctorId && m.Exam.ExamDate.Date == examDate && m.Exam.StartTime == examTime && m.ExamId != request.ExamId);
                if (doctorConflict)
                {
                    var conflictDoctor = _context.Doctors.Find(doctorId);
                    return Json(new { status = "conflict", message = "تعارض مراقب: الدكتور \"" + (conflictDoctor?.DoctorName ?? "") + "\" لديه امتحان آخر في نفس الوقت وهذا التاريخ" });
                }
            }

            exam.CourseId = request.CourseId;
            exam.RoomId = request.RoomId;
            exam.ExamDate = request.ExamDate;
            exam.StartTime = examTime;
            exam.EndTime = examTime.Add(TimeSpan.FromHours(2));

            _context.Monterings.RemoveRange(exam.Monitorings);
            foreach (var doctorId in allDoctorIds)
                _context.Monterings.Add(new Montering { DoctorId = doctorId, ExamId = exam.Id });

            _context.ExamExtraRooms.RemoveRange(exam.ExtraRooms);
            foreach (var extraRoomId in extraRoomIds)
                _context.ExamExtraRooms.Add(new ExamExtraRoom { RoomId = extraRoomId, ExamId = exam.Id });

            _context.SaveChanges();

            var roomNames = room.Name;
            if (extraRoomIds.Count > 0)
            {
                var extraRoomNames = _context.Rooms.Where(r => extraRoomIds.Contains(r.Id)).Select(r => r.Name).ToList();
                roomNames += " + " + string.Join(" + ", extraRoomNames);
            }

            return Json(new { status = "success", message = "تم تعديل الامتحان بنجاح في: " + roomNames });
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
