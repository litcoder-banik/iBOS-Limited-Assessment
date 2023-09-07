using System.ComponentModel.DataAnnotations;

namespace SalaryReview.Models
{
    public class EmployeeAttendance
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee employee { get; set; }

        [Required]
        public DateTime attendenceDate { get; set; }

        public int isPresent { get; set; } = 0;

        public int isAbsent { get; set; } = 0;  

        public int isOffday { get; set;} = 0;
    }
}
