using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var studentsInClass = from enroll in db.Enrolleds
                                  where enroll.ClassNavigation.ListingNavigation.Department == subject &&
                                        enroll.ClassNavigation.ListingNavigation.Number == num &&
                                        enroll.ClassNavigation.Season == season &&
                                        enroll.ClassNavigation.Year == year
                                  select new
                                  {
                                      fname = enroll.StudentNavigation.FName,
                                      lname = enroll.StudentNavigation.LName,
                                      uid = enroll.Student,
                                      dob = enroll.StudentNavigation.Dob,
                                      grade = enroll.Grade ?? "--"
                                  };

            return Json(studentsInClass);
        }


        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var assignmentsInCategory = from assignment in db.Assignments
                                        where assignment.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject &&
                                              assignment.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num &&
                                              assignment.CategoryNavigation.InClassNavigation.Season == season &&
                                              assignment.CategoryNavigation.InClassNavigation.Year == year &&
                                              (category == null || assignment.CategoryNavigation.Name == category)
                                        select new
                                        {
                                            aname = assignment.Name,
                                            cname = assignment.CategoryNavigation.Name,
                                            due = assignment.Due,
                                            submissions = assignment.Submissions.Count()
                                        };

            return Json(assignmentsInCategory);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var assignmentCategories = from ac in db.AssignmentCategories
                                       where ac.InClassNavigation.ListingNavigation.Department == subject &&
                                             ac.InClassNavigation.ListingNavigation.Number == num &&
                                             ac.InClassNavigation.Season == season &&
                                             ac.InClassNavigation.Year == year
                                       select new
                                       {
                                           name = ac.Name,
                                           weight = ac.Weight
                                       };

            return Json(assignmentCategories);
        }


        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var curCourse = db.Courses.FirstOrDefault(
                            c => c.Department == subject && c.Number == num);

            if (curCourse == null) return Json(new { success = false });

            var curClass = db.Classes.FirstOrDefault(
                           c => c.ListingNavigation.CatalogId == curCourse.CatalogId &&
                           c.Season == season && c.Year == year);

            if (curClass == null) return Json(new { success = false });

            var categoryWithGivenName = from ac in db.AssignmentCategories
                                        where ac.InClassNavigation.ListingNavigation.Department == subject &&
                                              ac.InClassNavigation.ListingNavigation.Number == num &&
                                              ac.InClassNavigation.Season == season &&
                                              ac.InClassNavigation.Year == year &&
                                              ac.Name == category
                                        select ac.CategoryId;

            if (categoryWithGivenName.Any())
            {
                // a category of the given class with the given name already exists
                return Json(new { success = false });
            }

            AssignmentCategory assignmentCategory = new AssignmentCategory()
            {
                Name = category,
                Weight = (uint) catweight,
                InClass = curClass.ClassId
            };

            db.AssignmentCategories.Add(assignmentCategory);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var curCourse = db.Courses.FirstOrDefault(
                            c => c.Department == subject && c.Number == num);

            if (curCourse == null) return Json(new { success = false });

            var curClass = db.Classes.FirstOrDefault(
                           c => c.ListingNavigation.CatalogId == curCourse.CatalogId &&
                           c.Season == season && c.Year == year);

            if (curClass == null) return Json(new { success = false });

            var curAC = db.AssignmentCategories.FirstOrDefault(ac =>
                                     ac.InClass == curClass.ClassId &&
                                     ac.Name == category);

            if (curAC == null) return Json(new { success = false });

            var assignmentExists = db.Assignments.FirstOrDefault(a =>
                                   a.Category == curAC.CategoryId &&
                                   a.Name == asgname);

            if (assignmentExists != null) return Json(new { success = false });

            Assignment assignment = new Assignment()
            {
                Category = curAC.CategoryId,
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
            };

            db.Assignments.Add(assignment);
            db.SaveChanges();

            var enrollments = db.Enrolleds.Where(e => e.Class == curClass.ClassId).ToList();

            foreach (var enrollment in enrollments)
            {
                GradeStudent(enrollment.Student, curClass.ClassId);
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var curCourse = db.Courses.FirstOrDefault(c =>
                         c.Department == subject && c.Number == num);

            if (curCourse == null) return Json(null);

            var curClass = db.Classes.FirstOrDefault(c =>
                           c.Listing == curCourse.CatalogId &&
                           c.Season == season &&
                           c.Year == year);

            if (curClass == null) return Json(null);

            var curAC = db.AssignmentCategories.FirstOrDefault(ac =>
                        ac.InClass == curClass.ClassId &&
                        ac.Name == category);

            if (curAC == null) return Json(null);

            var curAssignment = db.Assignments.FirstOrDefault(a =>
                                a.Category == curAC.CategoryId &&
                                a.Name == asgname);

            if (curAssignment == null) return Json(null);

            var submissionsToAssignment = db.Submissions
                             .Where(s => s.Assignment == curAssignment.AssignmentId)
                             .Select(s => new
                             {
                                 fname = s.StudentNavigation.FName,
                                 lname = s.StudentNavigation.LName,
                                 uid = s.Student,
                                 time = s.Time,
                                 score = s.Score
                             })
                             .ToList();

            return Json(submissionsToAssignment);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var curCourse = db.Courses.FirstOrDefault(c =>
                            c.Department == subject && c.Number == num);

            if (curCourse == null) return Json(new { success = false });

            var curClass = db.Classes.FirstOrDefault(c =>
                           c.Listing == curCourse.CatalogId &&
                           c.Season == season &&
                           c.Year == year);

            if (curClass == null) return Json(new { success = false });

            var curAC = db.AssignmentCategories.FirstOrDefault(ac =>
                        ac.InClass == curClass.ClassId &&
                        ac.Name == category);

            if (curAC == null) return Json(new { success = false });

            var curAssignment = db.Assignments.FirstOrDefault(a =>
                                a.Category == curAC.CategoryId &&
                                a.Name == asgname);

            if (curAssignment == null) return Json(new { success = false });

            var submissions = db.Submissions.FirstOrDefault(s =>
                              s.Assignment == curAssignment.AssignmentId &&
                              s.Student == uid);

            if (submissions == null) return Json(new { success = false });

            submissions.Score = (uint)score;
            db.SaveChanges();
            GradeStudent(uid, curClass.ClassId);

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var curProfessor = db.Professors.FirstOrDefault(
                               p => p.UId == uid);

            if (curProfessor == null) return Json(new List<object>());

            var myClasses = db.Classes.Where(c => c.TaughtBy == uid)
                            .Select(c => new
                            {
                                subject = c.ListingNavigation.Department,
                                number = c.ListingNavigation.Number,
                                name = c.ListingNavigation.Name,
                                season = c.Season,
                                year = c.Year
                            }).ToList();

            return Json(myClasses);
        }


        /// <summary>
        /// helper method
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="classId"></param>
        private void GradeStudent(string studentId, uint classId)
        {
            var enrollment = db.Enrolleds.FirstOrDefault(e =>
                            e.Class == classId &&
                            e.Student == studentId);

            if (enrollment != null)
            {
                var curAC = db.AssignmentCategories
                            .Where(ac => ac.InClass == enrollment.Class).ToList();
                double cumulativePoints = 0.0;
                double totalPoints = 0.0;

                foreach (var category in curAC)
                {
                    var assignments = db.Assignments.Where(a =>
                                      a.Category == category.CategoryId).ToList();
                    double categoryTotalPoints = 0.0;
                    double categoryTotalMaxPoints = 0.0;

                    foreach (var assignment in assignments)
                    {
                        categoryTotalMaxPoints += assignment.MaxPoints;

                        var submission = db.Submissions.SingleOrDefault(s =>
                                         s.Assignment == assignment.AssignmentId &&
                                         s.Student == studentId);

                        if (submission != null) categoryTotalPoints += submission.Score;
                    }

                    if (assignments.Count() != 0)
                    {
                        double categoryPercentage = categoryTotalMaxPoints > 0 ? categoryTotalPoints / categoryTotalMaxPoints : 0.0;
                        cumulativePoints += categoryPercentage * category.Weight;
                        totalPoints += category.Weight;
                    }
                }

                Console.WriteLine("cumulativePoints: " + cumulativePoints + " totalPoints: " + totalPoints);
                Console.WriteLine("cumulativePoints / totalPoints: " + cumulativePoints / totalPoints);

                enrollment.Grade = ConvertToLetterGrade(cumulativePoints / totalPoints * 100);
                db.SaveChanges();
            }
        }


        /// <summary>
        /// helper method
        /// </summary>
        /// <param name="grade"></param>
        /// <returns></returns>
        public static string ConvertToLetterGrade(double grade)
        {
            if (grade >= 93) return "A";
            if (grade >= 90) return "A-";
            if (grade >= 87) return "B+";
            if (grade >= 83) return "B";
            if (grade >= 80) return "B-";
            if (grade >= 77) return "C+";
            if (grade >= 73) return "C";
            if (grade >= 70) return "C-";
            if (grade >= 67) return "D+";
            if (grade >= 63) return "D";
            if (grade >= 60) return "D-";

            return "E";
        }
        /*******End code to modify********/
    }
}
