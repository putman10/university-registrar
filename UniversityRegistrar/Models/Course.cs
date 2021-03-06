﻿using System;
using MySql.Data.MySqlClient;
using UniversityRegistrar;
using System.Collections.Generic;

namespace UniversityRegistrar.Models
{
    public class Course
    {
        public string Name { get; set; }
        public string CRN { get; set; }
        public int Id { get; set; }

        public Course(string name, string crn, int id = 0)
        {
            Name = name;
            CRN = crn;
            Id = id;
        }

        public override bool Equals(System.Object otherCourse)
        {
            if (!(otherCourse is Course))
            {
                return false;
            }
            else
            {
                Course newCourse = (Course)otherCourse;
                bool idEquality = this.Id == newCourse.Id;
                bool nameEquality = this.Name == newCourse.Name;
                bool crnEquality = this.CRN == newCourse.CRN;

                return (idEquality && nameEquality && crnEquality);
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public void Save()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO courses (name, crn) VALUES (@Name, @CRN);";

            cmd.Parameters.AddWithValue("@Name", this.Name);
            cmd.Parameters.AddWithValue("@CRN", this.CRN);

            cmd.ExecuteNonQuery();
            Id = (int)cmd.LastInsertedId;
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public void AddStudent(Student newStudent)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO students_courses(course_id, student_id) VALUES(@CourseId, @StudentId);";


            cmd.Parameters.AddWithValue("@StudentId", newStudent.Id);
            cmd.Parameters.AddWithValue("@CourseId", this.Id);

            cmd.ExecuteNonQuery();
            Id = (int)cmd.LastInsertedId;
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public List<Student> GetStudents()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT students.* FROM courses
                                JOIN students_courses ON (courses.id = students_courses.course_id)
                                JOIN students ON (students_courses.student_id = students.id)
                                WHERE course_id = @CourseId;";

            cmd.Parameters.AddWithValue("@CourseId", this.Id);
            var rdr = cmd.ExecuteReader() as MySqlDataReader;

            List<Student> allStudents = new List<Student> { };

            while (rdr.Read())
            {
                int studentId = rdr.GetInt32(0);
                string studentName = rdr.GetString(1);
                DateTime enrollmentDate = rdr.GetDateTime(2);
                Student newStudent = new Student(studentName, enrollmentDate, studentId);
                allStudents.Add(newStudent);
            }

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }

            return allStudents;
        }

        public List<Student> GetAllStudents()
        {
            return Student.GetAll();
        }

        public static List<Course> GetAll()
        {
            List<Course> allCourses = new List<Course> { };
            MySqlConnection conn = DB.Connection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM courses";
            MySqlDataReader rdr = cmd.ExecuteReader() as MySqlDataReader;
            while (rdr.Read())
            {
                int courseId = rdr.GetInt32(0);
                string courseName = rdr.GetString(1);
                string courseCRN = rdr.GetString(2);

                Course newCourse = new Course(courseName, courseCRN, courseId);
                allCourses.Add(newCourse);
            }
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
            return allCourses;
        }

        public static Course Find(int id)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM courses WHERE id = @courseId;";

            cmd.Parameters.AddWithValue("@courseId", id);

            var rdr = cmd.ExecuteReader() as MySqlDataReader;

            int courseId = 0;
            string courseName = "";
            string CRN = "";

            while (rdr.Read())
            {
                courseId = rdr.GetInt32(0);
                courseName = rdr.GetString(1);
                CRN = rdr.GetString(2);
            }

            Course foundCourse = new Course(courseName, CRN, courseId);

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }

            return foundCourse;
        }

        public void Delete()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM courses WHERE id = @CourseId; DELETE FROM students_courses WHERE course_id = @CourseId;";

            cmd.Parameters.AddWithValue("@CourseId", this.Id);

            cmd.ExecuteNonQuery();
            if (conn != null)
            {
                conn.Close();
            }
        }

        public void Edit(string newName, string newCRN)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"UPDATE courses SET name = @Name, crn = @CRN  WHERE id = @searchId;";


            cmd.Parameters.AddWithValue("@Name", newName);
            cmd.Parameters.AddWithValue("@CRN", newCRN);
            cmd.Parameters.AddWithValue("@searchId", Id);

            cmd.ExecuteNonQuery();
            this.Name = newName;
            this.CRN = newCRN;
            //this.Id = id;

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }
    }
}
