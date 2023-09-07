using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SalaryReview.Models
{
    public class Employee
    {
        [Key]
        public int employeeId { get; set; }

        [Required(ErrorMessage = "This Feild is Required")]
        [StringLength(50, ErrorMessage = "Invalid Employee Name")]
        public string employeeName { get; set; }

        [Required(ErrorMessage = "This Feild is Required")]
        [StringLength(50, ErrorMessage = "Invalid Employee Name")]
        public string employeeCode { get; set; }

        [Required(ErrorMessage = "This Feild is Required")]
        public int employeeSalary { get; set; }

        [Required(ErrorMessage = "This Feild is Required")]
        public int supervisorId { get; set;}
    }
}
