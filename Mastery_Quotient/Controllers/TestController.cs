using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Mastery_Quotient.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;

        private readonly IConfiguration configuration;

        public TestController(ILogger<TestController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> TeacherWindowTest()
        {

            try
            {
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }

                    
                }
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                TestTeacher testTeacher = new TestTeacher(employee, disciplines, discipline, testParameters);
                return View(testTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> TeacherWindowTest(string nameTest, int parameterTest, int disciplineTest, bool deadlineCheck, string deadlineDate)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                Test test = new Test();

                test.NameTest = nameTest;
                test.DateCreatedTest = DateTime.Now;
                if(deadlineCheck)
                {
                    test.Deadline = null;
                }
                else
                {
                    test.Deadline = DateTime.Parse(deadlineDate);
                }
                test.DisciplineId = disciplineTest;
                test.EmployeeId = id;
                test.TestParameterId = parameterTest;
                test.IsDeleted = 0;
                test.Active = 0;
               


                StringContent content = new StringContent(JsonConvert.SerializeObject(test), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Tests", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("TestTeacher", "Test");
                    }
                    else
                    {
                        return RedirectToAction("TeacherWindowTest", "Test");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> TestTeacher()
        {
            try
            {
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();
                List<Test> tests = new List<Test>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Tests"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                    }
                }
                List<Test> testList = tests.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                TestViewTeacher testViewTeacher = new TestViewTeacher(employee, disciplines, discipline, testParameters, testList);
                return View(testViewTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        public IActionResult QuestionTest()
        {
            return View();
        }
    }
}
