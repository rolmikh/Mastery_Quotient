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
using System.Net.Http;
using API_Mastery_Quotient.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Mastery_Quotient.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;

        private readonly ILogger<HomeController> _logger;

        private readonly IValidator<Student> studentValidator;

        EmailService emailService = new EmailService();

        TokenService tokenService = new TokenService();

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

                        JObject responseJson = JObject.Parse(apiResponseStudent);
                        TokenService.token = (string)responseJson["access_token"];
                        await Authenticate((string)responseJson["username"]);
                        TempData["AuthUser"] = (int)responseJson["id"];

                        return RedirectToAction("MainStudent", "Student");
                    }
                    else if (responseEmployee.IsSuccessStatusCode)
                    {
                        string apiResponseEmployee = await responseEmployee.Content.ReadAsStringAsync();

                        JObject responseJson = JObject.Parse(apiResponseEmployee);
                        TokenService.token = (string)responseJson["access_token"];
                        await Authenticate((string)responseJson["username"]);

                        TempData["AuthUser"] = (int)responseJson["id"];

                        TokenService.role = (string)responseJson["role"];


                        if (TokenService.role == "Преподаватель")
                        {
                            return RedirectToAction("MaterialsTeacher", "Teacher");
                        }
                        else if (TokenService.role == "Администратор")
                        {
                            return RedirectToAction("MaterialsAdmin", "Admin");
                        }
                        else
                        {
                            TempData["Message"] = "Неверная электронная почта или пароль!";

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
                return BadRequest(ex.Message);
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

            return RedirectToAction("News", "Student");
        }

        /// <summary>
        /// POST запрос отправки кода на электронную почту при регистрации
        /// </summary>
        /// <param name="nameUser"></param>
        /// <param name="emailUser"></param>
        /// <param name="passwordUser"></param>
        /// <param name="groupUser"></param>
        /// <returns></returns>
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

                try
                {
                    var validationResult = await studentValidator.ValidateAsync(student);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("Registration", "Home");
                    }
                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("Registration", "Home");
                }


                await emailService.SendEmail(StudentRegistrationModel.EmailStudent, "Код подтверждения MastQuo", $"{key} - ваш код подтверждения электронной почты");
                TempData["Code"] = $"На электронную почту {StudentRegistrationModel.EmailStudent} был отправлен код подтверждения!";

                return View("Code");
            }
            catch(Exception e)
            {
                return BadRequest("Ошибка: " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы ввода кода
        /// </summary>
        /// <returns></returns>
        public ViewResult Code() => View();

        /// <summary>
        /// POST запрос регистрации пользователя
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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
                            TempData["Registration"] = "Успешная регистрация";
                            return RedirectToAction("Authorization", "Home");
                        }
                        else
                        {
                            TempData["Registration"] = "Неуспешная регистрация";

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
                return BadRequest(ex.Message);
            }
            

        }

        /// <summary>
        /// Загрузка представления страницы регистрации
        /// </summary>
        /// <returns></returns>
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
                return BadRequest(ex.Message);
            }
            
        }

        /// <summary>
        /// Функция "Забыли пароль", генерация ссылки для восстановления пароля
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdatePasswordNoAuth(string email)
        {
            var apiUrl = configuration["AppSettings:ApiUrl"];

            Student students = new Student();

            StringContent content = new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl + "Students/GetEmail?email=" + email);


                var studentData = await response.Content.ReadAsStringAsync();

                
                if (studentData == "По вашему запросу ничего не найдено!")
                {
                    Employee employees = new Employee();

                    using (var httpClientEmployee = new HttpClient())
                    {
                        var responseEmployee = await httpClientEmployee.GetAsync(apiUrl + "Employees/GetEmail?email=" + email);


                        var employeeData = await responseEmployee.Content.ReadAsStringAsync();

                       
                        if (employeeData == "По вашему запросу ничего не найдено!")
                        {
                            TempData["Error"] = "Такого пользователя не существует";
                            return RedirectToAction("ResetPasswordEmail", "Home");

                        }
                        var employeePart = employeeData.Split(" ");
                        var employeeEmail = employeePart[0];
                        var employeeID = int.Parse(employeePart[1]);
                        UserPasswordReset userPasswordResetEmployee = new UserPasswordReset();

                        userPasswordResetEmployee.id = employeeID;
                        userPasswordResetEmployee.email = employeeEmail;

                        StringContent contentNew = new StringContent(JsonConvert.SerializeObject(userPasswordResetEmployee), Encoding.UTF8, "application/json");

                        using (var httpClientNew = new HttpClient())
                        {
                            var responseNew = await httpClientNew.PostAsync(apiUrl + "Tokens/ResetPassword", contentNew);

                            if (responseNew.IsSuccessStatusCode)
                            {
                                responseNew.EnsureSuccessStatusCode();
                                var tokenData = await responseNew.Content.ReadAsStringAsync();

                                var token = JsonConvert.DeserializeObject<PasswordReset>(tokenData);

                                var resetPasswordLink = $"https://localhost:7106/Home/ResetPassword?token={token.Token}&email={userPasswordResetEmployee.email}&id={userPasswordResetEmployee.id}";

                                await emailService.SendEmail(email, "Сброс пароля MastQuo", $"<h2>Здравствуйте!</h2><p>Чтобы сбросить пароль для аккаунта {email} перейдите по следующей ссылке:</p><p>{resetPasswordLink}</p><p>Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо. Ваш пароль не будет изменен. Эта ссылка действительна в течение одного часа. Если вы не успеете воспользоваться ею в течение этого времени, вам придется запросить новую ссылку для сброса пароля.</p><br /><br /><br /><br /><br /><br /><h2> MastQuo</h2>");

                                return Ok($"На электронную почту {email} была отправлена ссылка. Для сброса пароля перейдите по этой ссылке и введите новый пароль!");
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }

                    }

                }
                var studentPart = studentData.Split(" ");
                var studentEmail = studentPart[0];
                var studentID = int.Parse(studentPart[1]);
                UserPasswordReset userPasswordReset = new UserPasswordReset();

                userPasswordReset.id = studentID;
                userPasswordReset.email = studentEmail;

                StringContent contentStudent = new StringContent(JsonConvert.SerializeObject(userPasswordReset), Encoding.UTF8, "application/json");

                using (var httpClientStudent = new HttpClient())
                {
                    var responseStudent = await httpClientStudent.PostAsync(apiUrl + "Tokens/ResetPassword", contentStudent);

                    if (responseStudent.IsSuccessStatusCode)
                    {
                        responseStudent.EnsureSuccessStatusCode();
                        var tokenData = await responseStudent.Content.ReadAsStringAsync();

                        var token = JsonConvert.DeserializeObject<PasswordReset>(tokenData);

                        var resetPasswordLink = $"https://localhost:7106/Home/ResetPassword?token={token.Token}&email={userPasswordReset.email}&id={userPasswordReset.id}";

                        await emailService.SendEmail(email, "Сброс пароля MastQuo", $"<h2>Здравствуйте!</h2><p>Чтобы сбросить пароль для аккаунта {email} перейдите по следующей ссылке:</p><p>{resetPasswordLink}</p><p>Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо. Ваш пароль не будет изменен. Эта ссылка действительна в течение одного часа. Если вы не успеете воспользоваться ею в течение этого времени, вам придется запросить новую ссылку для сброса пароля.</p><br /><br /><br /><br /><br /><br /><h2> MastQuo</h2>");

                        return Ok($"На электронную почту {email} была отправлена ссылка. Для сброса пароля перейдите по этой ссылке и введите новый пароль!");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            
        }

        /// <summary>
        /// Восстановление пароля, генерация ссылки для восстановления пароля
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdatePassword(string email)
        {
            var apiUrl = configuration["AppSettings:ApiUrl"];

           
            int id = int.Parse(TempData["AuthUser"].ToString());
            TempData.Keep("AuthUser");

           UserPasswordReset userPasswordReset =  new UserPasswordReset();

            userPasswordReset.id = id;
            userPasswordReset.email = email;

            StringContent content = new StringContent(JsonConvert.SerializeObject(userPasswordReset), Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(apiUrl + "Tokens/ResetPassword", content);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var tokenData = await response.Content.ReadAsStringAsync();

                    var token = JsonConvert.DeserializeObject<PasswordReset>(tokenData);

                    var resetPasswordLink = $"https://localhost:7106/Home/ResetPassword?token={token.Token}&email={userPasswordReset.email}&id={id}";

                    await emailService.SendEmail(email, "Сброс пароля MastQuo", $"<h2>Здравствуйте!</h2><p>Чтобы сбросить пароль для аккаунта {email} перейдите по следующей ссылке:</p><p>{resetPasswordLink}</p><p>Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо. Ваш пароль не будет изменен. Эта ссылка действительна в течение одного часа. Если вы не успеете воспользоваться ею в течение этого времени, вам придется запросить новую ссылку для сброса пароля.</p><br /><br /><br /><br /><br /><br /><h2> MastQuo</h2>");

                    return Ok($"На электронную почту {email} была отправлена ссылка. Для сброса пароля перейдите по этой ссылке и введите новый пароль!");
                }
                else
                {
                    return BadRequest();
                }
            }

        }

        /// <summary>
        /// Загрузка представления для восстановления пароля
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ResetPassword()
        {
            var email = HttpContext.Request.Query["email"];
            var id = HttpContext.Request.Query["id"];
            var token = HttpContext.Request.Query["token"];

            UserNewPassword userNewPassword = new UserNewPassword();
            userNewPassword.email = email;
            userNewPassword.id = int.Parse(id);
            userNewPassword.token = token;

            return View(userNewPassword);
        }

        /// <summary>
        /// POST запрос восстановления пароля
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <param name="newPassword"></param>
        /// <param name="repeatPassword"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id, string token ,string email, string newPassword, string repeatPassword)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                UserNewPassword userNewPassword = new UserNewPassword();
                userNewPassword.id = id;
                userNewPassword.email = email;
                userNewPassword.newPassword = newPassword;
                userNewPassword.repeatPassword = repeatPassword;
                userNewPassword.token = token;

                StringContent content = new StringContent(JsonConvert.SerializeObject(userNewPassword), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Tokens/NewPassword", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["NewPassword"] = "Пароль успешно изменен!";
                        return RedirectToAction("Authorization", "Home");
                    }
                    else
                    {
                        return BadRequest("Ошибка!");
                    }
                }
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Загрузка представления ввода электронной почты для не авторизованного пользователя
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetPasswordEmail()
        {
            return View();
        }


        public IActionResult PrivacyPolicy()
        {
            return View();
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
