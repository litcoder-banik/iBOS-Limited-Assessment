using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalaryReview.Models;
using System.Globalization;

namespace SalaryReview.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetEmployees()
        {
            try
            {
                var empDetails = _context.Employees.ToList();
                if (empDetails.Count == 0)
                {
                    return NotFound("Employee Not Found");
                }
                return Ok(empDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployee(int id)
        {
            try
            {
                var empDetail = _context.Employees.Find(id);
                if (empDetail == null)
                {
                    return NotFound($"Employee Not Found with id: {id}");
                }
                return Ok(empDetail);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult AddEmployee([FromBody] Employee employee)
        {
            try
            {
                _context.Add(employee);
                _context.SaveChanges();
                return Ok("Employee Created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch]
        public IActionResult UpdateEmployee(Employee employee)
        {
            try
            {
                if (employee == null || employee.employeeId == 0)
                {
                    return BadRequest("Invalid Employee Data");
                }
                else
                {
                    var existCode = _context.Employees.FirstOrDefault(e => e.employeeCode == employee.employeeCode && e.employeeId != employee.employeeId);
                    if(existCode != null)
                    {
                        return BadRequest("Employee Code Already Exist.");
                    }

                    var emp = _context.Employees.Find(employee.employeeId);
                    if(emp == null)
                    {
                        return NotFound($"Employee Not Found with id: {employee.employeeId}");
                    }

                    emp.employeeName = employee.employeeName;
                    emp.employeeCode = employee.employeeCode;
                    _context.Employees.Update(emp);
                    _context.SaveChanges();
                    return Ok("Employee Information Updated");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetThirdSalary()
        {
            try
            {
                var getThirdSalary = _context.Employees.OrderByDescending(e => e.employeeSalary).Skip(2).Take(1).ToList();

                if (getThirdSalary.Count == 0)
                {
                    return BadRequest("Failed to Get Salary");
                }
                else
                {
                    return Ok(getThirdSalary);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetEmployeeAttendence()
        {
            try
            {
                var empAttendence = _context.EmployeesAttendances.ToList();
                if (empAttendence.Count == 0)
                {
                    return NotFound("Employee Attendence Not Found");
                }
                return Ok(empAttendence);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id}")]
        public IActionResult GetEmployeeAttendence(int id)
        {
            try
            {
                var empAttendence = _context.EmployeesAttendances.Find(id);
                if (empAttendence == null)
                {
                    return NotFound($"Employee Not Found with id: {id}");
                }
                return Ok(empAttendence);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult AddEmployeeAttendence([FromBody] EmployeeAttendance employeeAttendance)
        {
            try
            {
                _context.Add(employeeAttendance);
                _context.SaveChanges();
                return Ok("Employee Attendence Created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public IActionResult GetSalary()
        {
            try
            {
                var employeesWithoutAbsences = _context.Employees.Where(e => !_context.EmployeesAttendances.Any(a => a.EmployeeId == e.employeeId && a.isPresent == 0 && a.isAbsent == 1 && a.isOffday == 0)).OrderByDescending(e => e.employeeSalary).ToList();
                return Ok(employeesWithoutAbsences);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message); 
            }
        }


        [HttpGet]
        public IActionResult GetReport()
        {
            try
            {
                var monthlyReport = _context.Employees
                .Select(employee => new
                {
                    Employee = employee,
                    AttendanceDate = GetEarliestAttendanceDate(employee.employeeId)
                })
                .Select(x => new MonthlyAttendanceReport
                {
                    EmployeeName = x.Employee.employeeName,
                    MonthName = GetMonthName(x.AttendanceDate), // Use the actual date field from your database
                    PayableSalary = x.Employee.employeeSalary, // You may need to calculate this based on your business logic
                    TotalPresent = _context.EmployeesAttendances.Count(a => a.EmployeeId == x.Employee.employeeId && a.isPresent == 1),
                    TotalAbsent = _context.EmployeesAttendances.Count(a => a.EmployeeId == x.Employee.employeeId && a.isAbsent == 1),
                    TotalOffday = _context.EmployeesAttendances.Count(a => a.EmployeeId == x.Employee.employeeId && a.isOffday == 1)
                })
                .ToList();

                if (monthlyReport.Count == 0)
                {
                    return NotFound("No Monthly Report Data Found");
                }
                else
                {
                    return Ok(monthlyReport);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private DateTime? GetEarliestAttendanceDate(int employeeId)
        {
            return _context.EmployeesAttendances
                .Where(a => a.EmployeeId == employeeId)
                .OrderBy(a => a.attendenceDate) // You may need to adjust the ordering based on your requirements
                .Select(a => a.attendenceDate)
                .FirstOrDefault();
        }

        private string GetMonthName(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToString("MMM");
            }

            // Return a default value or handle the case where the date is missing
            return "N/A"; // "N/A" indicates not available or an error state
        }



    }


    public class MonthlyAttendanceReport
    {
        public string EmployeeName { get; set; }
        public string MonthName { get; set; }
        public decimal PayableSalary { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalOffday { get; set; }
    }
}
