using Microsoft.EntityFrameworkCore;

namespace SalaryReview.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeAttendance> EmployeesAttendances { get; set; }
    }
}
