using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var departmentList = db.Departments.Select(d => new
            {
                name = d.Name,
                subject = d.Subject
            }).ToList();

            return Json(departmentList.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalogList = db.Departments.Select(d => new
            {
                subject = d.Subject,
                dname = d.Name,
                courses = d.Courses.Select(c => new
                {
                    number = c.Number,
                    cname = c.Name
                }).ToList()
            }).ToList();

            return Json(catalogList.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var classOfferings = db.Courses
                                .Where(c => c.Department == subject && c.Number == number)
                                .SelectMany(c => c.Classes)
                                .Select(cls => new
                                {
                                    season = cls.Season,
                                    year = cls.Year,
                                    location = cls.Location,
                                    start = cls.StartTime.ToString(),
                                    end = cls.EndTime.ToString(),
                                    fname = cls.TaughtByNavigation.FName,
                                    lname = cls.TaughtByNavigation.LName,
                                }).ToList();

            return Json(classOfferings.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var assignmentContents = db.Assignments
                                    .Where(a => a.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject &&
                                                a.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num &&
                                                a.CategoryNavigation.InClassNavigation.Season == season &&
                                                a.CategoryNavigation.InClassNavigation.Year == year &&
                                                a.CategoryNavigation.Name == category &&
                                                a.Name == asgname)
                                    .Select(a => a.Contents)
                                    .FirstOrDefault();

            return Content(assignmentContents ?? "");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var submissionText = db.Submissions
                                .Where(s => s.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject &&
                                            s.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num &&
                                            s.AssignmentNavigation.CategoryNavigation.InClassNavigation.Season == season &&
                                            s.AssignmentNavigation.CategoryNavigation.InClassNavigation.Year == year &&
                                            s.AssignmentNavigation.CategoryNavigation.Name == category &&
                                            s.AssignmentNavigation.Name == asgname &&
                                            s.Student == uid)
                                .Select(s => s.SubmissionContents)
                                .FirstOrDefault();

            return Content(submissionText ?? "");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var professor = db.Professors.FirstOrDefault(p => p.UId == uid);
            var student = db.Students.FirstOrDefault(s => s.UId == uid);
            var admin = db.Administrators.FirstOrDefault(a => a.UId == uid);

            if (professor != null)
            {
                var userObject = new
                {
                    fname = professor.FName,
                    lname = professor.LName,
                    uid = professor.UId,
                    department = professor.WorksIn
                };

                return Json(userObject);
            }

            if (student != null)
            {
                var userObject = new
                {
                    fname = student.FName,
                    lname = student.LName,
                    uid = student.UId,
                    department = student.Major
                };

                return Json(userObject);
            }

            if (admin != null)
            {
                var userObject = new
                {
                    fname = admin.FName,
                    lname = admin.LName,
                    uid = admin.UId
                };

                return Json(userObject);
            }

            return Json(new { success = false });
        }
        /*******End code to modify********/
    }
}
