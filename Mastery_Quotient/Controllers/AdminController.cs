using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Firebase.Storage;
using System.Net.Sockets;
using Mastery_Quotient.Service;
using Ganss.Xss;
using Microsoft.AspNetCore.Http.Extensions;
using FluentValidation;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json.Linq;

namespace Mastery_Quotient.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration configuration;

        private readonly IValidator<Employee> employeeValidator;

        private readonly IValidator<Student> studentValidator;

        EmailService emailService =  new EmailService();

        FirebaseService firebaseService = new FirebaseService();

        TokenService tokenService = new TokenService();

        public AdminController(IConfiguration configuration, IValidator<Employee> employeeValidator, ILogger<AdminController> logger, IValidator<Student> studentValidator)
        {
            this.configuration = configuration;
            this.employeeValidator = employeeValidator;
            _logger = logger;
            this.studentValidator = studentValidator;

        }

        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Загрузка представления главной страницы
        /// </summary>
        /// <returns></returns>

        public IActionResult Main()
        {
            return View();
        }

        //Employees
        /// <summary>
        /// Загрузка представления страницы с сотрудниками
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AdminWindowTeacher()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }

                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Employee> employees = new List<Employee>();
                List<Role> roles = new List<Role>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    if (TempData.ContainsKey("Search"))
                    {
                        employees = JsonConvert.DeserializeObject<List<Employee>>(TempData["Search"].ToString());
                    }
                    else if (TempData.ContainsKey("Filtration"))
                    {
                        employees = JsonConvert.DeserializeObject<List<Employee>>(TempData["Filtration"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Employees"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                        }
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Roles"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        roles = JsonConvert.DeserializeObject<List<Role>>(apiResponse);

                    }

                }



                EmployeeModelView employeeModel = new EmployeeModelView(employees, roles);
                return View(employeeModel);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        /// <summary>
        /// POST запрос для поиска преподавателей
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AdminWindowTeacher(string Search)
        {
           
            if (Search != null)
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<Employee> employees = new List<Employee>();


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Employees/Search?search=" + Search);

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);

                    TempData["Search"] = JsonConvert.SerializeObject(employees);

                    if(employees.Count == 0)
                    {
                        TempData["Message"] = "По вашему запросу ничего не найдено";
                        return RedirectToAction("AdminWindowTeacher", "Admin");

                    }

                    return RedirectToAction("AdminWindowTeacher", "Admin");
                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("AdminWindowTeacher", "Admin");
            }

        }

        /// <summary>
        /// POST запрос фильтрации преподавателей
        /// </summary>
        /// <param name="roleTeacher"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationTeacher(int roleTeacher)
        {
            try
            {
                if (roleTeacher != 0)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Employee> employees = new List<Employee>();

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        using (var response = await httpClient.GetAsync(apiUrl + "Employees/Filtration?idRole=" + roleTeacher))
                        {
                            var apiResponse = await response.Content.ReadAsStringAsync();
                            employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                            TempData["Filtration"] = JsonConvert.SerializeObject(employees);

                            if (employees.Count == 0)
                            {
                                TempData["Message"] = "По вашему запросу ничего не найдено";
                                return RedirectToAction("AdminWindowTeacher", "Admin");

                            }
                            return RedirectToAction("AdminWindowTeacher", "Admin");


                        }

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("AdminWindowTeacher", "Admin");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            

        }

        /// <summary>
        /// Загрузка представления страницы добавления преподавателей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TeacherNew()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, TokenService.role);
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Employee> employees = new List<Employee>();
                List<Role> roles = new List<Role>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    if (HttpContext.Session.Keys.Contains("Employees"))
                    {
                        employees = JsonConvert.DeserializeObject<List<Employee>>(HttpContext.Session.GetString("Employees"));
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Employees"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                        }
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Roles"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        roles = JsonConvert.DeserializeObject<List<Role>>(apiResponse);

                    }

                }
                EmployeeModelView employeeModel = new EmployeeModelView(employees, roles);
                return View(employeeModel);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос добавления преподавателей
        /// </summary>
        /// <param name="nameUser"></param>
        /// <param name="emailUser"></param>
        /// <param name="passwordUser"></param>
        /// <param name="roleUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TeacherNew(string nameUser, string emailUser, string passwordUser, int roleUser)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                string[] subs = nameUser.Split();


                Employee employee = new Employee();

                if (subs.Length >= 2)
                {
                    employee.SurnameEmployee = subs[0];

                    if (subs.Length >= 3)
                    {
                        employee.NameEmployee = subs[1];
                        employee.MiddleNameEmployee = string.Join(" ", subs.Skip(2));
                    }
                    else
                    {
                        employee.NameEmployee = subs[1];
                        employee.MiddleNameEmployee = string.Empty;
                    }
                }
                else
                {
                    return BadRequest("Введите корректное ФИО!");
                }

                employee.EmailEmployee = emailUser;
                employee.PasswordEmployee = passwordUser;
                employee.IsDeleted = 0;
                employee.RoleId = roleUser;

               
                try
                {
                    var validationResult = await employeeValidator.ValidateAsync(employee);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("TeacherNew", "Admin");
                    }
                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TeacherNew", "Admin");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PostAsync(apiUrl + "Employees", content);

                    if (response.IsSuccessStatusCode)
                    {

                        await emailService.SendEmail(employee.EmailEmployee, "Добро пожаловать в MastQuo", $"<h2>Здравствуйте, {employee.NameEmployee} {employee.MiddleNameEmployee}</h2><p>Ваш аккаунт в системе успешно создан. Ниже приведены ваши данные для входа: <p>Электронная почта: {employee.EmailEmployee}</p><p>Пароль: {passwordUser}</p><br/><p>После первого входа в систему мы рекомендуем вам изменить пароль на более надежный и легко запоминающийся. Пароль можно изменить в вашем личном кабинете.</p>Желаем вам удачи и эффективной работы!</p><br /><br /><br /><h2> MastQuo</h2>");

                   

                        return RedirectToAction("AdminWindowTeacher", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("TeacherNew", "Admin");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос изменения данных
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="surnameUser"></param>
        /// <param name="nameUser"></param>
        /// <param name="middleNameUser"></param>
        /// <param name="emailUser"></param>
        /// <param name="roleUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTeacher(int idUser,string surnameUser, string nameUser, string middleNameUser, string emailUser,int roleUser)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }

                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Employees/" + idUser);
                    response.EnsureSuccessStatusCode();

                    var employeeTestData = await response.Content.ReadAsStringAsync();

                    var employee = JsonConvert.DeserializeObject<Employee>(employeeTestData);

                    employee.SurnameEmployee = surnameUser;
                    employee.NameEmployee = nameUser;
                    employee.MiddleNameEmployee = middleNameUser;
                    employee.EmailEmployee = emailUser;
                    employee.RoleId = roleUser;
                    try
                    {
                        var validationResult = await employeeValidator.ValidateAsync(employee);

                        if (!validationResult.IsValid)
                        {
                            var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                            TempData["ErrorValidation"] = errorMessages;
                            return RedirectToAction("UpdateTeacher", "Admin");
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessages = new List<string> { ex.Message };
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateTeacher", "Admin");
                    }
                    string json = JsonConvert.SerializeObject(employee);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Employees/" + idUser, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("AdminWindowTeacher", "Admin");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы обновления преподавателей
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTeacher(int id)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = null;
                List<Role> roles = new List<Role>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Roles"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        roles = JsonConvert.DeserializeObject<List<Role>>(apiResponse);
                    }
                }

                EmployeeModelView employeeModel = new EmployeeModelView(new List<Employee> { employee }, roles);
                return View(employeeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос удаления данных
        /// </summary>
        /// <param name="IdEmployee"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int IdEmployee)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.DeleteAsync(apiUrl + "Employees/" + IdEmployee))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("AdminWindowTeacher", "Admin");
            }
            catch(Exception ex)
            {
                return BadRequest("Ошибка удаления данных! " + ex.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы подробной информации о преподавателях
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DetailsTeacher(int id)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employees = new Employee();
                List<Role> roles = new List<Role>();
                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<EmployeeStudyGroup> employeeStudyGroups = new List<EmployeeStudyGroup>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<Discipline> disciplines = new List<Discipline>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Roles"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        roles = JsonConvert.DeserializeObject<List<Role>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "EmployeeStudyGroups"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employeeStudyGroups = JsonConvert.DeserializeObject<List<EmployeeStudyGroup>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                }

                Role role = roles.Find(n => n.IdRole == employees.RoleId);
                List<EmployeeStudyGroup> employeeStudyGroupsList = employeeStudyGroups.Where(n => n.EmployeeId == id).ToList();
                List<DisciplineEmployee> disciplineEmployeesList = disciplineEmployees.Where(n => n.EmployeeId == id).ToList();
                EmployeeModelDetails employeeModelDetails = new EmployeeModelDetails(employees, role, studyGroups, employeeStudyGroupsList, disciplineEmployeesList, disciplines);
                return View(employeeModelDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы личного кабинета
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PersonalAccountAdmin()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Role> roles = new List<Role>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Roles"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        roles = JsonConvert.DeserializeObject<List<Role>>(apiResponse);
                    }
                }
                Role role = roles.Find(n => n.IdRole == employee.RoleId);
                EmployeePersonalAccountModel employeePersonalAccountModel = new EmployeePersonalAccountModel(employee, role);
                return View(employeePersonalAccountModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос обновления данных в личном кабинете
        /// </summary>
        /// <param name="surnameUser"></param>
        /// <param name="nameUser"></param>
        /// <param name="middleNameUser"></param>
        /// <param name="emailUser"></param>
        /// <param name="passwordUser"></param>
        /// <param name="saltUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonalAccountAdmin(string surnameUser, string nameUser, string middleNameUser, string emailUser)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Employees/" + id);
                    response.EnsureSuccessStatusCode();

                    var employeeData = await response.Content.ReadAsStringAsync();

                    var employee = JsonConvert.DeserializeObject<Employee>(employeeData);
                    employee.SurnameEmployee = surnameUser;
                    employee.NameEmployee = nameUser;
                    employee.MiddleNameEmployee = middleNameUser;
                    employee.EmailEmployee = emailUser;

                    try
                    {
                        var validationResult = await employeeValidator.ValidateAsync(employee);

                        if (!validationResult.IsValid)
                        {
                            var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                            TempData["ErrorValidation"] = errorMessages;
                            return RedirectToAction("PersonalAccountAdmin", "Admin");
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessages = new List<string> { ex.Message };
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("PersonalAccountAdmin", "Admin");
                    }

                    string json = JsonConvert.SerializeObject(employee);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Employees/" + id, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("PersonalAccountAdmin", "Admin");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// POST запрос обновления фотографии в личном кабинете
        /// </summary>
        /// <param name="photo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> updatePhoto(IFormFile photo)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                if (photo == null || photo.Length == 0)
                {
                    TempData["ErrorPhoto"] = "Файл не был загружен";
                    return RedirectToAction("PersonalAccountAdmin", "Admin");

                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);


                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Employees/" + id);
                    response.EnsureSuccessStatusCode();
                    var employeeData = await response.Content.ReadAsStringAsync();

                    var employee = JsonConvert.DeserializeObject<Employee>(employeeData);

                    employee.PhotoEmployee = await firebaseService.Upload(photo.OpenReadStream(), fileName);

                    string json = JsonConvert.SerializeObject(employee);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Employees/" + id, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("PersonalAccountAdmin", "Admin");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        

        /// <summary>
        /// Выход из аккаунта
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Очищаем кэш для предотвращения возможности вернуться назад
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            TempData.Remove("AuthUser");
           

            return RedirectToAction("News", "Student");
        }

        //Students
        /// <summary>
        /// Загрузка представления страницы просмотра студентов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AdminWindowStudent()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Student> students = new List<Student>();
                List<StudyGroup> studyGroups = new List<StudyGroup>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    if (TempData.ContainsKey("Search"))
                    {
                        students = JsonConvert.DeserializeObject<List<Student>>(TempData["Search"].ToString());
                    }
                    else if(TempData.ContainsKey("Filtration"))
                    {
                        students = JsonConvert.DeserializeObject<List<Student>>(TempData["Filtration"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Students"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            students = JsonConvert.DeserializeObject<List<Student>>(apiResponse);
                        }
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);

                    }

                }



                StudentModelView studentModelView = new StudentModelView(students, studyGroups);
                return View(studentModelView);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        /// <summary>
        /// POST запрос поиска студентов
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AdminWindowStudent(string Search)
        {
            try
            {
                
                if (Search != null)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];
                    List<Student> students = new List<Student>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.GetAsync(apiUrl + "Students/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        students = JsonConvert.DeserializeObject<List<Student>>(apiResponse);

                        TempData["Search"] = JsonConvert.SerializeObject(students);
                        if (students.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("AdminWindowStudent", "Admin");

                        }
                        return RedirectToAction("AdminWindowStudent", "Admin");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("AdminWindowStudent", "Admin");
                }
            }
            catch (Exception ex)
            { 
                return BadRequest(ex.Message);
            
            }

           

        }

        /// <summary>
        /// POST запрос фильтрации студентов.
        /// </summary>
        /// <param name="groupUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationStudent(int groupUser)
        {
            try
            {
                
                if (groupUser != 0)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Student> studentsList = new List<Student>();

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        using (var response = await httpClient.GetAsync(apiUrl + "Students/Filtration?idStudyGroup=" + groupUser))
                        {
                            var apiResponse = await response.Content.ReadAsStringAsync();
                            studentsList = JsonConvert.DeserializeObject<List<Student>>(apiResponse);
                            TempData["Filtration"] = JsonConvert.SerializeObject(studentsList);

                            if (studentsList.Count == 0)
                            {
                                TempData["Message"] = "По вашему запросу ничего не найдено";
                                return RedirectToAction("AdminWindowStudent", "Admin");

                            }

                            return RedirectToAction("AdminWindowStudent", "Admin");


                        }

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("AdminWindowStudent", "Admin");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос изменения данных студента
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="surnameUser"></param>
        /// <param name="nameUser"></param>
        /// <param name="middleNameUser"></param>
        /// <param name="emailUser"></param>
        /// <param name="studyGroupUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateStudent(int idUser,string surnameUser, string nameUser, string middleNameUser, string emailUser , int studyGroupUser)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Students/" + idUser);
                    response.EnsureSuccessStatusCode();
                    var studentData = await response.Content.ReadAsStringAsync();

                    var student = JsonConvert.DeserializeObject<Student>(studentData);

                    student.SurnameStudent = surnameUser;
                    student.NameStudent = nameUser;
                    student.MiddleNameStudent = middleNameUser;
                    student.EmailStudent = emailUser;
                    student.StudyGroupId = studyGroupUser;

                    try
                    {
                        var validationResult = await studentValidator.ValidateAsync(student);

                        if (!validationResult.IsValid)
                        {
                            var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                            TempData["ErrorValidation"] = errorMessages;
                            return RedirectToAction("UpdateStudent", "Admin");
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessages = new List<string> { ex.Message };
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateStudent", "Admin");
                    }
                    string json = JsonConvert.SerializeObject(student);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Students/" + idUser, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("AdminWindowStudent", "Admin");
                    }
                    else
                    {
                        return BadRequest("Ошибка изменения данных");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Загрузка представления страницы изменения данных студента
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateStudent(int id)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Student student = null;
                List<StudyGroup> studyGroups = new List<StudyGroup>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Students/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        student = JsonConvert.DeserializeObject<Student>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }
                }

                StudentModelView studentModelView = new StudentModelView(new List<Student> { student }, studyGroups);
                return View(studentModelView);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос удаления студента
        /// </summary>
        /// <param name="IdStudent"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int IdStudent)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.DeleteAsync(apiUrl + "Students/" + IdStudent))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("AdminWindowStudent", "Admin");
            }
            catch
            {
                return BadRequest("Ошибка удаления данных!");
            }
        }

        /// <summary>
        /// Загрузка представления страницы подробной информации о студенте
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DetailsStudent(int id)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Student student = new Student();
                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<Course> courses = new List<Course>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Students/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        student = JsonConvert.DeserializeObject<Student>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                }
                StudyGroup group = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId);
                Course course = courses.Find(n => n.IdCourse == group.CourseId);
                StudentModelDetails studentModelDetails = new StudentModelDetails(student, group, course);

                return View(studentModelDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Загрузка представления страницы просмотра материалов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MaterialsAdmin()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Material> materials = new List<Material>();
                List<Discipline> disciplines = new List<Discipline>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    if (TempData.ContainsKey("Search"))
                    {
                        materials = JsonConvert.DeserializeObject<List<Material>>(TempData["Search"].ToString());
                    }
                    else if (TempData.ContainsKey("Filtration"))
                    {
                        materials = JsonConvert.DeserializeObject<List<Material>>(TempData["Filtration"].ToString());

                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Materials"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                        }
                    }
                }

                MaterialModelView materialModelView = new MaterialModelView(materials, disciplines, typeMaterials);

                return View(materialModelView);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }




        /// <summary>
        /// Загрузка представления страницы с файлом материала
        /// </summary>
        /// <param name="nameFile"></param>
        /// <returns></returns>
        public IActionResult FileMaterial(string nameFile)
        {
            if (!TempData.ContainsKey("AuthUser"))
            {
                return RedirectToAction("Authorization", "Home");
            }

            int id = int.Parse(TempData["AuthUser"].ToString());

            TempData.Keep("AuthUser");

            if (nameFile.Contains("youtu.be") || nameFile.Contains("youtube"))
            {
                string[] fileNameParts = nameFile.Split("https://youtu.be/");
                string fileNameID = fileNameParts[1];
                ViewBag.FileType = "youtube";
                ViewBag.NameFile = fileNameID;

            }
            else
            {
                string[] fileNameParts = nameFile.Split('?'); // Разделить имя файла и токен
                string fileNameWithoutToken = fileNameParts[0]; // Получить только имя файла без токена
                string fileType = Path.GetExtension(fileNameWithoutToken).ToLower();
                if (fileType == ".pdf")
                {
                    ViewBag.FileType = "pdf";
                    ViewBag.NameFile = nameFile;
                }
                else
                {
                    ViewBag.FileType = "other";
                    ViewBag.NameFile = nameFile;
                }
            }
            return View();
        }

        

        /// <summary>
        /// Загрузка представления страницы просмотра тестирований
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AdminTestView()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
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
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }
                    if (TempData.ContainsKey("Search"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["Search"].ToString());
                    }
                    else if (TempData.ContainsKey("Filtration"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["Filtration"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Tests/Active"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                        }
                    }
                    
                }
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                TestViewTeacher testViewTeacher = new TestViewTeacher(employee, disciplines, discipline, testParameters, tests);
                return View(testViewTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос поиска тестирований
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AdminTestView(string Search)
        {
            try
            {
               
                if (Search != null)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];
                    List<Test> tests = new List<Test>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.GetAsync(apiUrl + "Tests/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);

                        TempData["Search"] = JsonConvert.SerializeObject(tests);

                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("AdminTestView", "Admin");

                        }

                        return RedirectToAction("AdminTestView", "Admin");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("AdminTestView", "Admin");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }



        }

        /// <summary>
        /// Загрузка представления страницы просмотра одного тестирования
        /// </summary>
        /// <param name="testId"></param>
        /// <returns></returns>
        public async Task<IActionResult> AdminOneTestView(int testId)
        {

            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                TempData["testID"] = testId;
                TempData.Keep("testID");


                int testID = int.Parse(TempData["testID"].ToString());
                TempData.Keep("testID");


                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Test test = new Test();
                List<TypeQuestion> typeQuestions = new List<TypeQuestion>();
                List<Question> questions = new List<Question>();
                List<TestQuestion> testQuestions = new List<TestQuestion>();
                List<AnswerOption> answerOptions = new List<AnswerOption>();
                List<QuestionAnswerOption> questionAnswerOptions = new List<QuestionAnswerOption>();
                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Tests/" + testID))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        test = JsonConvert.DeserializeObject<Test>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeQuestions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeQuestions = JsonConvert.DeserializeObject<List<TypeQuestion>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Questions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        questions = JsonConvert.DeserializeObject<List<Question>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestQuestions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testQuestions = JsonConvert.DeserializeObject<List<TestQuestion>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "AnswerOptions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        answerOptions = JsonConvert.DeserializeObject<List<AnswerOption>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "QuestionAnswerOptions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        questionAnswerOptions = JsonConvert.DeserializeObject<List<QuestionAnswerOption>>(apiResponse);
                    }

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
                }
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                List<TestQuestion> testQuestionsList = testQuestions.Where(n => n.TestId == test.IdTest).ToList();

                ViewTestModel viewTestModel = new ViewTestModel(test, typeQuestions, questions, testQuestionsList, answerOptions, questionAnswerOptions, employee, disciplines, discipline);

                return View(viewTestModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// POST запрос фильтрации тестирований
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationTest(int parameterId, int disciplineID)
        {
            try
            {
                if (parameterId != 0 || disciplineID != 0)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Test> tests = new List<Test>();

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        if (parameterId != 0 && disciplineID == 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Tests/Filtration?idParameter=" + parameterId))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                            }
                        }
                        else if (parameterId == 0 && disciplineID != 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Tests/Filtration?idDiscipline=" + disciplineID))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                            }
                        }
                        else
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + $"Tests/Filtration?idDiscipline={disciplineID}&idParameter={parameterId}"))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                            }

                        }
                        TempData["Filtration"] = JsonConvert.SerializeObject(tests);

                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("AdminTestView", "Admin");

                        }

                        return RedirectToAction("AdminTestView", "Admin");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("AdminTestView", "Admin");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос поиска материалов
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MaterialsAdmin(string Search)
        {
            try
            {
                if (Search != null)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];
                    List<Material> materials = new List<Material>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.GetAsync(apiUrl + "Materials/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);

                        TempData["Search"] = JsonConvert.SerializeObject(materials);

                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("MaterialsAdmin", "Admin");

                        }

                        return RedirectToAction("MaterialsAdmin", "Admin");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("MaterialsAdmin", "Admin");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }



        }

        /// <summary>
        /// POST запрос фильтрации материалов
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationMaterial(int typeId, int disciplineID)
        {
            try
            {
                if (typeId != 0 || disciplineID != 0)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Администратор");
                        if (refreshToken != null)
                        {
                            TokenService.token = refreshToken;
                        }
                        else
                        {
                            return RedirectToAction("Authorization", "Home");
                        }
                    }
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Material> materials = new List<Material>();

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        if (typeId != 0 && disciplineID == 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Materials/Filtration?idType=" + typeId))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                            }
                        }
                        else if (typeId == 0 && disciplineID != 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Materials/Filtration?idDiscipline=" + disciplineID))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                            }
                        }
                        else
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + $"Materials/Filtration?idDiscipline={disciplineID}&idType={typeId}"))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                            }

                        }
                        TempData["Filtration"] = JsonConvert.SerializeObject(materials);

                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("MaterialsAdmin", "Admin");

                        }

                        return RedirectToAction("MaterialsAdmin", "Admin");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("MaterialsAdmin", "Admin");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }

}
