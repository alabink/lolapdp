using System;
using System.Collections.Generic;
using System.Linq;

namespace lolapdp.Models
{
    public class CourseManagement : ICourseManagement
    {
        private List<Course> _courses;
        private readonly ICSVService _csvService;

        public CourseManagement(ICSVService csvService)
        {
            _csvService = csvService;
            _courses = new List<Course>();
            LoadCourses();
        }

        private void LoadCourses()
        {
            try
            {
                _courses = _csvService.ReadCSV<Course>("Data/courses.csv");
            }
            catch (Exception)
            {
                _courses = new List<Course>();
            }
        }

        private void SaveCourses()
        {
            try
            {
                _csvService.WriteCSV("Data/courses.csv", _courses);
            }
            catch (Exception)
            {
                // Log exception
            }
        }

        public List<Course> GetAllCourses()
        {
            return _courses.ToList();
        }

        public Course GetCourseById(int id)
        {
            return _courses.FirstOrDefault(c => c.Id == id);
        }

        public bool AddCourse(Course course)
        {
            if (_courses.Any(c => c.Id == course.Id))
                return false;

            _courses.Add(course);
            SaveCourses();
            return true;
        }

        public bool UpdateCourse(Course course)
        {
            var index = _courses.FindIndex(c => c.Id == course.Id);
            if (index == -1)
                return false;

            _courses[index] = course;
            SaveCourses();
            return true;
        }

        public bool DeleteCourse(int id)
        {
            var course = _courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
                return false;

            _courses.Remove(course);
            SaveCourses();
            return true;
        }
    }
} 