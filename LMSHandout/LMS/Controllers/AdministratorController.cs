using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            bool departmentExists = db.Departments.Any(depart => depart.Subject == subject);

            if (departmentExists)
            {
                return Json(new { success = false });
            }

            // If the department does not exist, create a new department with
            // the provided subject code and full name
            Department department = new Department
            {
                Name = name,
                Subject = subject
            };

            db.Departments.Add(department);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courseList = db.Courses
                        .Where(c => c.Department == subject)
                        .Select(c => new { number = c.Number, name = c.Name })
                        .ToList();

            return Json(courseList);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professorList = db.Professors
                                  .Where(p => p.WorksIn == subject)
                                  .Select(p => new { lname = p.LName, fname = p.FName, uid = p.UId })
                                  .ToList();

            return Json(professorList);
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // Check if the course with the given subject and number already exists
            var courseExists = db.Courses.Any(course => course.Number == (uint) number && course.Department == subject);

            if (courseExists)
            {
                return Json(new { success = false });
            }

            var course = new Course
            {
                Name = name,
                Number = (uint) number,
                Department = subject
            };

            db.Courses.Add(course);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var classExists = from c in db.Classes
                              where c.ListingNavigation.DepartmentNavigation.Subject == subject
                              where c.ListingNavigation.Number == number
                              where c.Season == season
                              where c.Year == year &&
                              (
                              TimeOnly.FromDateTime(start).IsBetween(c.StartTime, c.EndTime) ||
                              TimeOnly.FromDateTime(end).IsBetween(c.StartTime, c.EndTime) ||
                              TimeOnly.FromDateTime(start) == c.StartTime ||
                              TimeOnly.FromDateTime(end) == c.EndTime)
                              where c.Location == location
                              select c;

            if (!classExists.Any())
            {
                Class newClass = new Class
                {
                    Season = season,
                    Year = (uint)year,
                    Location = location,
                    StartTime = TimeOnly.FromDateTime(start),
                    EndTime = TimeOnly.FromDateTime(end),
                    Listing = (from l in db.Courses where l.Department == subject && l.Number == (uint)number select l.CatalogId).FirstOrDefault(),
                    TaughtBy = instructor
                };

                db.Add(newClass);
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        /*******End code to modify********/
    }
}

