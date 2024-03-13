using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Mastery_Quotient.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            this.configuration = configuration;
            _logger = logger;
        }

        //public ContentResult OnGet()
        //{
        //    var apiURl = configuration["AppSettings:ApiUrl"];

        //    return Content(apiURl);
        //}

        private readonly ILogger<HomeController> _logger;

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Authorization()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorization(string emailUser, string passwordUser)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee()
                {
                    EmailEmployee = emailUser,
                    PasswordEmployee = passwordUser
                };

                Student student = new Student()
                {
                    EmailStudent = emailUser,
                    PasswordStudent = passwordUser
                };

                StringContent contentStudent = new StringContent(JsonConvert.SerializeObject(student), Encoding.UTF8, "application/json");
                StringContent contentEmployee = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var responseStudent = await httpClient.PostAsync(apiUrl + "Students/Authorization", contentStudent);
                    var responseEmployee = await httpClient.PostAsync(apiUrl + "Employees/Authorization", contentEmployee);

                    if (responseStudent.IsSuccessStatusCode)
                    {
                        string apiResponseStudent = await responseStudent.Content.ReadAsStringAsync();
                        var students = JsonConvert.DeserializeObject<Student>(apiResponseStudent);

                        await Authenticate(students.EmailStudent);
                        TempData["AuthUser"] = students.IdStudent;

                        return RedirectToAction("MainStudent", "Student");
                    }
                    else if (responseEmployee.IsSuccessStatusCode)
                    {
                        string apiResponseEmployee = await responseEmployee.Content.ReadAsStringAsync();
                        var employees = JsonConvert.DeserializeObject<Employee>(apiResponseEmployee);

                        await Authenticate(employees.EmailEmployee);
                        TempData["AuthUser"] = employees.IdEmployee;

                        if (employees.RoleId == 1)
                        {
                            return RedirectToAction("MainTeacher", "Teacher");
                        }
                        else if (employees.RoleId == 2)
                        {
                            return RedirectToAction("Main", "Admin");
                        }
                        else
                        {
                            return BadRequest();
                        }


                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
            }
            catch (Exception ex) 
            {
                return BadRequest();
            }

            
        }


        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData.Remove("AuthUser");

            return RedirectToAction("Authorization", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Registration(string nameUser, string emailUser, string passwordUser, int groupUser)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                string[] subs = nameUser.Split();


                Student student = new Student();

                if (subs.Length >= 2)
                {
                    student.SurnameStudent = subs[0];

                    if (subs.Length >= 3)
                    {
                        student.NameStudent = subs[1];
                        student.MiddleNameStudent = string.Join(" ", subs.Skip(2));
                    }
                    else
                    {
                        student.NameStudent = subs[1];
                        student.MiddleNameStudent = string.Empty;
                    }
                }

                student.EmailStudent = emailUser;
                student.PasswordStudent = passwordUser;
                student.IsDeleted = 0;
                student.StudyGroupId = groupUser;


                StringContent contentStudent = new StringContent(JsonConvert.SerializeObject(student), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var responseStudent = await httpClient.PostAsync(apiUrl + "Students", contentStudent);

                    if (responseStudent.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Registration", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            

        }

        public async Task <IActionResult> Registration()
        {

            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<StudyGroup> studyGroups = new List<StudyGroup>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                        
                    }
                }

                return View(studyGroups);
                
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
