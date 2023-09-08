using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
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

        [Authorize]
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

        /*[Authorize]*/
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


        /*API#01*/
        [Authorize]
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

        /*API#02*/
        /*[Authorize]*/
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

        /*API#03*/
        /*[Authorize]*/
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

        /*API#04*/
        /*[Authorize]*/
        [HttpGet("monthlyattendance")]
        public IActionResult GetMonthlyAttendanceReport()
        {
            try
            {
                var monthlyReport = _context.Employees
                    .ToList() 
                    .Select(employee => new MonthlyAttendanceReport
                    {
                        EmployeeName = employee.employeeName,
                        MonthName = GetMonthName(employee.employeeId),
                        PayableSalary = employee.employeeSalary,
                        TotalPresent = _context.EmployeesAttendances.Count(a => a.EmployeeId == employee.employeeId && a.isPresent == 1),
                        TotalAbsent = _context.EmployeesAttendances.Count(a => a.EmployeeId == employee.employeeId && a.isAbsent == 1),
                        TotalOffday = _context.EmployeesAttendances.Count(a => a.EmployeeId == employee.employeeId && a.isOffday == 1)
                    })
                    .ToList();

                if (monthlyReport.Count == 0)
                {
                    return NotFound("No Monthly Report Data Found");
                }

                return Ok(monthlyReport);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetMonthName(int employeeId)
        {
            var monthName = _context.EmployeesAttendances
                .Where(a => a.EmployeeId == employeeId)
                .OrderBy(a => a.attendenceDate)
                .Select(a => a.attendenceDate.Month)
                .FirstOrDefault();

            if (monthName != 0)
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthName);
            }

            return "N/A";
        }




        /*API#05*/
        /*[Authorize]*/
        [HttpGet("employeehierarchy/{employeeId}")]
        public IActionResult GetEmployeeHierarchy(int employeeId)
        {
            try
            {
                var employee = _context.Employees.FirstOrDefault(e => e.employeeId == employeeId);

                if (employee == null)
                {
                    return NotFound("Employee Not Found");
                }

                var hierarchy = GetHierarchy(employeeId);

                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private EmployeeHierarchy GetHierarchy(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.employeeId == employeeId);

            if (employee == null)
            {
                return null;
            }

            var hierarchy = new EmployeeHierarchy
            {
                EmployeeId = employee.employeeId,
                EmployeeName = employee.employeeName,
                Subordinates = new List<EmployeeHierarchy>()
            };

            var subordinates = _context.Employees.Where(e => e.supervisorId == employeeId).ToList();

            foreach (var subordinate in subordinates)
            {
                var subordinateHierarchy = GetHierarchy(subordinate.employeeId);
                if (subordinateHierarchy != null)
                {
                    hierarchy.Subordinates.Add(subordinateHierarchy);
                }
            }

            return hierarchy;
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

    public class EmployeeHierarchy
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public List<EmployeeHierarchy> Subordinates { get; set; }
    }

}
