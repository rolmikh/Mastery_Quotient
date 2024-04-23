using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json.Linq;
using Mastery_Quotient.Service;
using FluentValidation;
using Mastery_Quotient.ModelsValidation;

namespace Mastery_Quotient.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;

        private readonly ILogger<HomeController> _logger;

        private readonly IValidator<Student> studentValidator;

        EmailService emailService = new EmailService();

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger, IValidator<Student> studentValidator)
        {
            this.configuration = configuration;
            _logger = logger;
            this.studentValidator = studentValidator;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Authorization()
        {
            return View();
        }


        /// <summary>
        /// POST запрос авторизации пользователя
        /// </summary>
        /// <param name="emailUser"></param>
        /// <param name="passwordUser"></param>
        /// <returns></returns>
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
                        //await GoogleDriveService.InitializeDriveServiceAsync(emailUser, passwordUser);

                        return RedirectToAction("MainStudent", "Student");
                    }
                    else if (responseEmployee.IsSuccessStatusCode)
                    {
                        string apiResponseEmployee = await responseEmployee.Content.ReadAsStringAsync();
                        var employees = JsonConvert.DeserializeObject<Employee>(apiResponseEmployee);

                        await Authenticate(employees.EmailEmployee);
                        TempData["AuthUser"] = employees.IdEmployee;
                        //await GoogleDriveService.InitializeDriveServiceAsync(emailUser, passwordUser);


                        if (employees.RoleId == 1)
                        {
                            return RedirectToAction("MaterialsTeacher", "Teacher");
                        }
                        else if (employees.RoleId == 2)
                        {
                            return RedirectToAction("MaterialsAdmin", "Admin");
                        }
                        else
                        {
                            return BadRequest();
                        }


                    }
                    else
                    {
                        TempData["Message"] = "Неверная электронная почта или пароль!";

                        return RedirectToAction("Authorization", "Home");
                    }
                }
            }
            catch (Exception ex) 
            {
                return BadRequest();
            }

            
        }

        /// <summary>
        /// Метод, который создает идентификационные данные пользователя на основе предоставленного имени пользователя и аутентифицирует его
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод выхода из аккаунта
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData.Remove("AuthUser");

            return RedirectToAction("Authorization", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Code(string nameUser, string emailUser, string passwordUser, int groupUser)
        {
            try
            {
                int key = 0;


                key = new Random().Next(100000,999999);
                string[] subs = nameUser.Split();


                if (subs.Length >= 2)
                {
                    StudentRegistrationModel.SurnameStudent = subs[0];

                    if (subs.Length >= 3)
                    {
                        StudentRegistrationModel.NameStudent = subs[1];
                        StudentRegistrationModel.MiddleNameStudent = string.Join(" ", subs.Skip(2));
                    }
                    else
                    {
                        StudentRegistrationModel.NameStudent = subs[1];
                        StudentRegistrationModel.MiddleNameStudent = string.Empty;
                    }
                }

                StudentRegistrationModel.EmailStudent = emailUser;
                StudentRegistrationModel.PasswordStudent = passwordUser;
                StudentRegistrationModel.IsDeleted = 0;
                StudentRegistrationModel.StudyGroupId = groupUser;
                StudentRegistrationModel.Key = key.ToString();

                Student student = new Student();

                student.SurnameStudent = StudentRegistrationModel.SurnameStudent;
                student.NameStudent = StudentRegistrationModel.NameStudent;
                student.MiddleNameStudent = StudentRegistrationModel.MiddleNameStudent;

                student.EmailStudent = StudentRegistrationModel.EmailStudent;
                student.PasswordStudent = StudentRegistrationModel.PasswordStudent;
                student.IsDeleted = StudentRegistrationModel.IsDeleted;
                student.StudyGroupId = StudentRegistrationModel.StudyGroupId;

                var validationResult = await studentValidator.ValidateAsync(student);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("Registration", "Home");
                }

                await emailService.SendEmail(StudentRegistrationModel.EmailStudent, "Код подтверждения MastQuo", $"{key} - ваш код подтверждения электронной почты");

                return View("Code");
            }
            catch(Exception e)
            {
                return BadRequest("Ошибка: " + e.Message);
            }
        }


        public ViewResult Code() => View();


        [HttpPost]
        public async Task<IActionResult> Registration(string key)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                string keyKey = StudentRegistrationModel.Key;
                if (key.Equals(keyKey))
                {

                    Student student = new Student();

                    student.SurnameStudent = StudentRegistrationModel.SurnameStudent;
                    student.NameStudent = StudentRegistrationModel.NameStudent;
                    student.MiddleNameStudent = StudentRegistrationModel.MiddleNameStudent;

                    student.EmailStudent = StudentRegistrationModel.EmailStudent;
                    student.PasswordStudent = StudentRegistrationModel.PasswordStudent;
                    student.IsDeleted = StudentRegistrationModel.IsDeleted;
                    student.StudyGroupId = StudentRegistrationModel.StudyGroupId;

                    

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
                else
                {
                    return BadRequest("Ошибка кода подтверждения!");
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
                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
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
