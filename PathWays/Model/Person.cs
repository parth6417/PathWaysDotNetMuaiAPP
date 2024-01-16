using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Model
{
    public class Person
    {
        public string Person_Id { get; set; }

        [Required(ErrorMessage = "Please Enter FirstName")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "Please Enter MiddleName")]
        public string Middle_Name { get; set; }

        [Required(ErrorMessage = "Please Enter LastName")]
        public string Last_Name { get; set; }


        [Required(ErrorMessage = "Please Enter Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please Enter Email")]
        [EmailAddress]
        public string Email_1 { get; set; }


        [Required(ErrorMessage = "Please Enter Mobile No.")]
        [Phone]
        public string Mobile_Phone { get; set; }

        public string? Address_1 { get; set; }


        [Required(ErrorMessage = "Please Enter Postal_Code")]
        [Phone]
        public string Postal_Code { get; set; }
        public int TypeId { get; set; }

    }
}
