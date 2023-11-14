using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS_lab4
{
    public class Student 
    {
        public int Id { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public int Age { get; set; }
        public bool Status { get; set; }

        public Student(int id, string fname, string lname, int age, bool status)
        {
            Id = id;
            Fname = fname;
            Lname = lname;
            Age = age;
            Status = status;
        }
    }

}
