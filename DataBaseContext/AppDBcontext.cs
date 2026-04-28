using Microsoft.EntityFrameworkCore;
using pro_exam.Models;

namespace pro_exam.DataBaseContext
{
    public class AppDBcontext : DbContext
    {
        public AppDBcontext(DbContextOptions<AppDBcontext> options) : base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique constraint on DoctorName
            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.DoctorName)
                .IsUnique();

            // Student <-> Course (Many-to-Many via StudentCourse)
            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.RegisteredCourses)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.EnrolledStudents)
                .HasForeignKey(sc => sc.CourseId);

            // Doctor <-> Course (Many-to-Many via DoctorCourse)
            modelBuilder.Entity<DoctorCourse>()
                .HasKey(dc => new { dc.DoctorId, dc.CourseId });

            modelBuilder.Entity<DoctorCourse>()
                .HasOne(dc => dc.Doctor)
                .WithMany(d => d.DoctorCourses)
                .HasForeignKey(dc => dc.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorCourse>()
                .HasOne(dc => dc.Course)
                .WithMany(c => c.DoctorCourses)
                .HasForeignKey(dc => dc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Exam -> Course
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Exam -> Room
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Room)
                .WithMany()
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Montering -> Doctor (proctoring assignment)
            modelBuilder.Entity<Montering>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.Monitorings)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Montering -> Exam
            modelBuilder.Entity<Montering>()
                .HasOne(m => m.Exam)
                .WithMany(e => e.Monitorings)
                .HasForeignKey(m => m.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamExtraRoom -> Exam
            modelBuilder.Entity<ExamExtraRoom>()
                .HasOne(r => r.Exam)
                .WithMany(e => e.ExtraRooms)
                .HasForeignKey(r => r.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamExtraRoom -> Room
            modelBuilder.Entity<ExamExtraRoom>()
                .HasOne(r => r.Room)
                .WithMany()
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<DoctorCourse> DoctorCourses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Montering> Monterings { get; set; }
        public DbSet<ExamExtraRoom> ExamExtraRooms { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
