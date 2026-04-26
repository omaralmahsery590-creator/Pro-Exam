using Microsoft.AspNetCore.Mvc.Rendering;

namespace pro_exam.ViewModel
{
    public class AddDoctorViewModel
    {
        public string DoctorName { get; set; }

        public int? SelectedDoctorId { get; set; }

        public List<int> SelectedCourseIds { get; set; } = new();

        public List<SelectListItem> AvailableCourses { get; set; } = new();

        public List<SelectListItem> AvailableDoctors { get; set; } = new();
    }

    public class DoctorCourseRowViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Specialization { get; set; }
        public int Level { get; set; }
        public int NumberOfStudents { get; set; }
    }

    public class AddDoctorCourseRequest
    {
        public string DoctorName { get; set; }
        public int? CourseId { get; set; }
        public string CourseName { get; set; }
        public int StudentsCount { get; set; }
        public string Specialization { get; set; }
        public int Level { get; set; }
    }

    public class UpdateDoctorCourseRequest
    {
        public int OriginalDoctorId { get; set; }
        public int OriginalCourseId { get; set; }
        public string DoctorName { get; set; }
        public string CourseName { get; set; }
        public string Specialization { get; set; }
        public int Level { get; set; }
        public int StudentsCount { get; set; }
    }

    public class DeleteDoctorCourseRequest
    {
        public int DoctorId { get; set; }
        public int CourseId { get; set; }
    }

    public class AddExamRequest
    {
        public int DoctorId { get; set; }
        public int CourseId { get; set; }
        public DateTime ExamDate { get; set; }
        public string ExamTime { get; set; }
        public int ExpectedStudents { get; set; }
        public int RoomId { get; set; }
        public List<int> ExtraDoctorIds { get; set; } = new();
        public List<int> ExtraRoomIds { get; set; } = new();
    }

    public class EditExamRequest
    {
        public int ExamId { get; set; }
        public int DoctorId { get; set; }
        public int CourseId { get; set; }
        public DateTime ExamDate { get; set; }
        public string ExamTime { get; set; }
        public int ExpectedStudents { get; set; }
        public int RoomId { get; set; }
        public List<int> ExtraDoctorIds { get; set; } = new();
        public List<int> ExtraRoomIds { get; set; } = new();
    }
}
