using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pro_exam.DataBaseContext;
using pro_exam.Models;
using pro_exam.ViewModel;

namespace pro_exam.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]//يمنع أي شخص غير مسجل الدخول من الوصول لهذه الصفحة.
    public class DoctorController : Controller
    {
        private readonly AppDBcontext _context;

        public DoctorController(AppDBcontext context)
        {
            _context = context;
        }

        public IActionResult AddNewDoctor()
        {
            var rows = _context.DoctorCourses
                .Include(dc => dc.Doctor)
                .Include(dc => dc.Course)
                .OrderBy(dc => dc.Doctor.DoctorName)
                .ThenBy(dc => dc.Course.CourseName)
                .Select(dc => new DoctorCourseRowViewModel
                {
                    DoctorId = dc.DoctorId,
                    DoctorName = dc.Doctor.DoctorName,
                    CourseId = dc.CourseId,
                    CourseName = dc.Course.CourseName,
                    Specialization = dc.Course.Specialization,
                    Level = dc.Course.Level,
                    NumberOfStudents = dc.Course.NumberOfStudents
                })
                .ToList();

            return View(rows);
        }

        [HttpGet]
        public IActionResult GetDoctorCourses(int doctorId)
        {
            var courseIds = _context.DoctorCourses
                .Where(dc => dc.DoctorId == doctorId)
                .Select(dc => dc.CourseId)
                .ToList();

            return Json(courseIds);
        }

        [HttpPost]
        public IActionResult AddDoctorCourse([FromBody] AddDoctorCourseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.DoctorName))
                return Json(new { success = false, message = "يرجى إدخال اسم الدكتور" });
            if (string.IsNullOrWhiteSpace(request?.CourseName))
                return Json(new { success = false, message = "يرجى إدخال اسم المادة" });
            if (string.IsNullOrWhiteSpace(request?.Specialization))
                return Json(new { success = false, message = "يرجى اختيار التخصص" });
            if (request.Level < 1 || request.Level > 4)
                return Json(new { success = false, message = "يرجى اختيار مستوى صحيح" });

            var doctorName = request.DoctorName.Trim();//إزالة أي مسافات زائدة قبل أو بعد النص
            var courseName = request.CourseName.Trim();
            var specialization = request.Specialization.Trim();

            Course course = null;
            if (request.CourseId.HasValue && request.CourseId.Value > 0)
                course = _context.Courses.FirstOrDefault(c => c.Id == request.CourseId.Value);

            if (course == null)
                course = _context.Courses.FirstOrDefault(c => c.CourseName == courseName);

            if (course == null)
            {
                if (!request.CourseId.HasValue || request.CourseId.Value <= 0)
                    return Json(new { success = false, message = "يرجى إدخال رقم المادة لإضافة مادة جديدة" });

                course = new Course
                {
                    Id = request.CourseId.Value,
                    CourseName = courseName,
                    Specialization = specialization,
                    Level = request.Level,
                    NumberOfStudents = request.StudentsCount
                };
                _context.Courses.Add(course);
                _context.SaveChanges();
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorName == doctorName);
            if (doctor == null)
            {
                doctor = new Doctor { DoctorName = doctorName };
                _context.Doctors.Add(doctor);
                _context.SaveChanges();
            }

            bool exists = _context.DoctorCourses.Any(dc => dc.DoctorId == doctor.Id && dc.CourseId == course.Id);
            if (exists)
                return Json(new { success = false, isDuplicate = true, message = "هذه المادة مضافة مسبقاً لهذا الدكتور" });

            _context.DoctorCourses.Add(new DoctorCourse { DoctorId = doctor.Id, CourseId = course.Id });
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "تمت إضافة المادة بنجاح",
                data = new
                {
                    doctorId = doctor.Id,
                    doctorName = doctor.DoctorName,
                    courseId = course.Id,
                    courseName = course.CourseName,
                    specialization = course.Specialization,
                    level = course.Level,
                    studentsCount = course.NumberOfStudents
                }
            });
        }

        [HttpPost]
        public IActionResult UpdateDoctorCourse([FromBody] UpdateDoctorCourseRequest request)
        {
            if (request == null)
                return Json(new { success = false, message = "بيانات غير صحيحة" });
            if (string.IsNullOrWhiteSpace(request.DoctorName))
                return Json(new { success = false, message = "يرجى إدخال اسم الدكتور" });
            if (string.IsNullOrWhiteSpace(request.CourseName))
                return Json(new { success = false, message = "يرجى إدخال اسم المادة" });
            if (string.IsNullOrWhiteSpace(request.Specialization))
                return Json(new { success = false, message = "يرجى اختيار التخصص" });
            if (request.Level < 1 || request.Level > 4)
                return Json(new { success = false, message = "يرجى اختيار مستوى صحيح" });

            var doctor = _context.Doctors.Find(request.OriginalDoctorId);
            if (doctor == null)
                return Json(new { success = false, message = "الدكتور غير موجود" });

            var course = _context.Courses.Find(request.OriginalCourseId);
            if (course == null)
                return Json(new { success = false, message = "المادة غير موجودة" });

            var newDoctorName = request.DoctorName.Trim();
            if (doctor.DoctorName != newDoctorName)
            {
                bool nameConflict = _context.Doctors.Any(d => d.DoctorName == newDoctorName && d.Id != doctor.Id);
                if (nameConflict)
                    return Json(new { success = false, message = "يوجد دكتور آخر بهذا الاسم بالفعل" });
                doctor.DoctorName = newDoctorName;
            }

            course.CourseName = request.CourseName.Trim();
            course.Specialization = request.Specialization.Trim();
            course.Level = request.Level;
            course.NumberOfStudents = request.StudentsCount;

            try
            {
                _context.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = "تم تحديث السجل بنجاح",
                    data = new
                    {
                        doctorId = doctor.Id,
                        doctorName = doctor.DoctorName,
                        courseId = course.Id,
                        courseName = course.CourseName,
                        specialization = course.Specialization,
                        level = course.Level,
                        studentsCount = course.NumberOfStudents
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحفظ: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteDoctorCourse([FromBody] DeleteDoctorCourseRequest request)
        {
            if (request == null)
                return Json(new { success = false, message = "بيانات غير صحيحة" });

            var dc = _context.DoctorCourses
                .FirstOrDefault(x => x.DoctorId == request.DoctorId && x.CourseId == request.CourseId);
            if (dc == null)
                return Json(new { success = false, message = "السجل غير موجود" });

            _context.DoctorCourses.Remove(dc);
            _context.SaveChanges();

            return Json(new { success = true, message = "تم حذف السجل بنجاح" });
        }
    }
}
