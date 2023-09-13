using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;

        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var curStudent = db.Students.FirstOrDefault(s => s.UId == uid);

            if (curStudent == null) return Json(new List<object>());

            var classesEnrolledIn = db.Enrolleds
                                    .Where(e => e.Student == uid)
                                    .Select(e => new
                                    {
                                        subject = e.ClassNavigation.ListingNavigation.Department,
                                        number = e.ClassNavigation.ListingNavigation.Number,
                                        name = e.ClassNavigation.ListingNavigation.Name,
                                        season = e.ClassNavigation.Season,
                                        year = e.ClassNavigation.Year,
                                        grade = e.Grade ?? "--"
                                    }).ToList();

            return Json(classesEnrolledIn);
        }


        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var curStudent = db.Students.FirstOrDefault(s => s.UId == uid);

            if (curStudent == null) return Json(new List<object>());

            var enrolledClasses = db.Classes.FirstOrDefault(c =>
                                  c.ListingNavigation.Department == subject &&
                                  c.ListingNavigation.Number == num &&
                                  c.Season == season &&
                                  c.Year == year &&
                                  c.Enrolleds.Any(e => e.Student == uid));

            if (enrolledClasses == null) return Json(new List<object>());

            var assignmentsInClass = db.Assignments
                             .Where(a => a.CategoryNavigation.InClass == enrolledClasses.ClassId)
                             .Select(a => new
                             {
                                 aname = a.Name,
                                 cname = a.CategoryNavigation.Name,
                                 due = a.Due,
                                 score = db.Submissions
                                 .Where(s => s.Assignment == a.AssignmentId &&
                                        s.Student == uid)
                                 .Select(s => (ushort?)s.Score)
                                 .SingleOrDefault()
                             })
                             .ToList();

            return Json(assignmentsInClass);
        }


        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var curAssignment = (from course in db.Courses
                                join c in db.Classes on course.CatalogId equals c.Listing
                                join enrollment in db.Enrolleds on c.ClassId equals enrollment.Class
                                join student in db.Students on enrollment.Student equals student.UId
                                join ac in db.AssignmentCategories on c.ClassId equals ac.InClass
                                join assignm in db.Assignments on ac.CategoryId equals assignm.Category
                                where student.UId == uid &&
                                      course.Department == subject &&
                                      course.Number == num &&
                                c.Season == season && c.Year == year &&
                                ac.Name == category && assignm.Name == asgname
                                select assignm).SingleOrDefault();

            if (curAssignment == null) return Json(new { success = false });

            // Check if the student already submitted for the given assignment
            var submission = (from s in db.Submissions
                                   where s.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.DepartmentNavigation.Subject == subject &&
                                         s.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num &&
                                         s.AssignmentNavigation.CategoryNavigation.InClassNavigation.Season == season &&
                                         s.AssignmentNavigation.CategoryNavigation.InClassNavigation.Year == year &&
                                         s.AssignmentNavigation.CategoryNavigation.Name == category &&
                                         s.AssignmentNavigation.Name == asgname &&
                                         s.Student == uid
                                   select s).SingleOrDefault();

            // If the student already submitted, update the submission contents and time
            if (submission != null)
            {
                submission.Time = DateTime.Now;
                submission.SubmissionContents = contents;
            }
            else
            {
                submission = new Submission
                {
                    Student = uid,
                    Assignment = curAssignment.AssignmentId,
                    Time = DateTime.Now,
                    SubmissionContents = contents,
                    Score = 0
                };
                db.Submissions.Add(submission);
            }

            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var curStudent = db.Students.FirstOrDefault(s => s.UId == uid);

            if (curStudent == null) return Json(new { success = false });

            var curCourse = db.Courses.FirstOrDefault(c =>
                            c.Department == subject && c.Number == num);

            if (curCourse == null) return Json(new { success = false });

            var curClass = db.Classes.FirstOrDefault(c =>
                           c.Listing == curCourse.CatalogId &&
                           c.Season == season &&
                           c.Year == year);

            if (curClass == null) return Json(new { success = false });

            var existingEnrollment = db.Enrolleds.FirstOrDefault(e =>
                                     e.Student == uid &&
                                     e.Class == curClass.ClassId);

            if (existingEnrollment != null) return Json(new { success = false });

            bool hasAssignments = db.Assignments.Any(a =>
                                  a.CategoryNavigation.InClass == curClass.ClassId);

            var enrollment = new Enrolled
            {
                Student = uid,
                Class = curClass.ClassId,
                Grade = "--"
            };

            db.Enrolleds.Add(enrollment);
            db.SaveChanges();

            return Json(new { success = true });
        }


        private readonly Dictionary<string, double> gradeConversion = new Dictionary<string, double>
        {
            { "A+", 4.0 }, { "A", 4.0 }, { "A-", 3.7 }, { "B+", 3.3 }, { "B", 3.0 },
            { "B-", 2.7 }, { "C+", 2.3 }, { "C", 2.0 }, { "C-", 1.7 }, { "D+", 1.3 },
            { "D", 1.0 }, { "D-", 0.7 }, { "E", 0.0 }
        };


        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var curStudent = db.Students.FirstOrDefault(s => s.UId == uid);

            if (curStudent == null) return Json(new { gpa = 0.0 });

            // verify if the student is enrolled (if no, gpa == 0) 
            var studentIsEnrolled = from St in db.Enrolleds
                                    where St.Student == uid
                                    select St.Student;

            if (!studentIsEnrolled.Any())
            {
                // Student is not enrolled in any classes, GPA is 0.0
                return Json(new { gpa = "0.0" });
            }

            // student is enrolled, grab the grades the student is enrolled in (excluding "--")
            var gradesFromClasses = from E in db.Enrolleds
                                    where E.Student == uid && E.Grade != "--"
                                    select E.Grade;

            if (!gradesFromClasses.Any())
            {
                // Student is not graded in any classes, GPA is 0.0
                return Json(new { gpa = "0.0" });
            }

            var totalCreditHours = gradesFromClasses.Count() * 4;
            var totalGradePoints = 0.0;

            foreach (var letter in gradesFromClasses)
            {
                // Lookup the grade-point conversion value for each grade and calculate the grade points
                if (gradeConversion.TryGetValue(letter, out double gradePoint))
                {
                    totalGradePoints += gradePoint * 4;
                }
            }

            double studentGPA = Math.Round(totalGradePoints / totalCreditHours, 2, MidpointRounding.AwayFromZero);

            return Json(new { gpa = studentGPA });
        }
        /*******End code to modify********/
    }
}
