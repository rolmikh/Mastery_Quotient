using Mastery_Quotient.Models;
using Mastery_Quotient.ModelsValidation;
using Mastery_Quotient.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FluentValidation;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Linq;

namespace Mastery_Quotient.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<StudentController> _logger;

        private readonly IValidator<Student> studentValidator;

        FirebaseService firebaseService = new FirebaseService();

        TokenService tokenService = new TokenService();

        public StudentController(IConfiguration configuration, ILogger<StudentController> logger, IValidator<Student> studentValidator)
        {
            this.configuration = configuration;
            _logger = logger;
            this.studentValidator = studentValidator;
        }

        

        /// <summary>
        /// Загрузка представления личного кабинета студента
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PersonalAccountStudent()
        {
            try
            {
                if (!TempData.ContainsKey("AuthUser"))
                {
                    return RedirectToAction("Authorization", "Home");
                }
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

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Student student = new Student();
                List<StudyGroup> studyGroups = new List<StudyGroup>();

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
                }
                StudyGroup studentGroup = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId);
                StudentPersonalAccountModel studentPersonalAccountModel = new StudentPersonalAccountModel(student, studentGroup);
                return View(studentPersonalAccountModel);
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
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonalAccountStudent(string surnameUser, string nameUser, string middleNameUser, string emailUser)
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
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Students/" + id);
                    response.EnsureSuccessStatusCode();
                    var studentData = await response.Content.ReadAsStringAsync();

                    var student = JsonConvert.DeserializeObject<Student>(studentData);

                    student.SurnameStudent = surnameUser;
                    student.NameStudent = nameUser;
                    student.EmailStudent = emailUser;
                    student.MiddleNameStudent = middleNameUser;

                    
                    try
                    {
                        var validationResult = await studentValidator.ValidateAsync(student);

                        if (!validationResult.IsValid)
                        {
                            var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                            TempData["ErrorValidation"] = errorMessages;
                            return RedirectToAction("PersonalAccountStudent", "Student");
                        }

                    }
                    catch (Exception ex)
                    {
                        var errorMessages = new List<string> { ex.Message };
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("PersonalAccountStudent", "Student");
                    }
                    string json = JsonConvert.SerializeObject(student);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Students/" + id, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("PersonalAccountStudent", "Student");
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
        /// POST запрос обновления фотографии профиля
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
                if (photo == null || photo.Length == 0)
                {
                    return BadRequest("Файл не был загружен");
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);


                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];




                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl + "Students/" + id);
                    response.EnsureSuccessStatusCode();
                    var studentData = await response.Content.ReadAsStringAsync();

                    var student = JsonConvert.DeserializeObject<Student>(studentData);

                    student.PhotoStudent = await firebaseService.Upload(photo.OpenReadStream(), fileName);

                    string json = JsonConvert.SerializeObject(student);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Students/" + id, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("PersonalAccountStudent", "Student");
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

        /// <summary>
        /// Загрузка представления страницы материалов студента
        /// </summary>
        /// <returns></returns>
        
        public async Task<IActionResult> MaterialStudent()
        {
            try
            {
                if (!TempData.ContainsKey("AuthUser"))
                {
                    return RedirectToAction("Authorization", "Home");
                }
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
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Material> materials = new List<Material>();
                List<Discipline> disciplines = new List<Discipline>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();
                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups = new List<DisciplineOfTheStudyGroup>();
                List<Employee> employees = new List<Employee>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                Student student = new Student();

                

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
                    var group = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId).IdStudyGroup;
                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineOfTheStudyGroups/Filtration?idStudyGroup=" + group))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineOfTheStudyGroups = JsonConvert.DeserializeObject<List<DisciplineOfTheStudyGroup>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }
                   
                }
                StudyGroup studyGroup = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId);

                StudentMaterialModel studentMaterialModel = new StudentMaterialModel(materials, disciplines, typeMaterials, studyGroup, disciplineOfTheStudyGroups,employees,disciplineEmployees, student);
                return View(studentMaterialModel);
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Загрузка представления страницы тестирований студента
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TestStudent()
        {
            try
            {

                if (!TempData.ContainsKey("AuthUser"))
                {
                    return RedirectToAction("Authorization", "Home");
                }
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
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Employee> employees = new List<Employee>();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();
                List<Test> tests = new List<Test>();
                Student student = new Student();
                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups = new List<DisciplineOfTheStudyGroup>();
                List<StudentTest> studentTests = new List<StudentTest>();

                

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
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
                        using (var response = await httpClient.GetAsync(apiUrl + "Tests"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                        }
                    }
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
                    var group = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId).IdStudyGroup;
                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineOfTheStudyGroups/Filtration?idStudyGroup=" + group))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineOfTheStudyGroups = JsonConvert.DeserializeObject<List<DisciplineOfTheStudyGroup>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "StudentTests"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studentTests = JsonConvert.DeserializeObject<List<StudentTest>>(apiResponse);
                    }



                }
                StudyGroup studyGroup = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId);
                TestViewStudent testViewStudent = new TestViewStudent(employees, disciplines, disciplineEmployees, testParameters, tests, student, disciplineOfTheStudyGroups, studyGroup, studentTests);
                return View(testViewStudent);
                
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
        public async Task<IActionResult> OneTestStudent(int testId)
        {
            try
            {
                if (!TempData.ContainsKey("AuthUser"))
                {
                    return RedirectToAction("Authorization", "Home");
                }

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
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                TempData["testID"] = testId;
                TempData.Keep("testID");


                int testID = int.Parse(TempData["testID"].ToString());
                TempData.Keep("testID");



                var apiUrl = configuration["AppSettings:ApiUrl"];

                Test test = new Test();
                List<TypeQuestion> typeQuestions = new List<TypeQuestion>();
                List<TestParameter> testParameters = new List<TestParameter>();
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
        /// POST запрос поиска материалов на странице материалов
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MaterialStudent(string Search)
        {
            try
            {
                if (Search != null)
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
                    List<Material> materials = new List<Material>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.GetAsync(apiUrl + "Materials/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);

                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            
                            return RedirectToAction("MaterialStudent", "Student");

                        }

                        TempData["Search"] = JsonConvert.SerializeObject(materials);
                       
                        return RedirectToAction("MaterialStudent", "Student");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";
                    return RedirectToAction("MaterialStudent", "Student");
                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }



        }

        /// <summary>
        /// POST запрос поиска материалов на странице новостей
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MaterialStudentNews(string Search)
        {
            try
            {
                if (Search != null)
                {
                    
                    var apiUrl = configuration["AppSettings:ApiUrl"];
                    List<Material> materials = new List<Material>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {

                        var response = await httpClient.GetAsync(apiUrl + "Materials/NoAuthSearch?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);


                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("News", "Student");

                        }
                        TempData["SearchMaterial"] = JsonConvert.SerializeObject(materials);
                        return RedirectToAction("News", "Student");
                       
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";
                    return RedirectToAction("News", "Student");

                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }
        }

        /// <summary>
        /// POST запрос поиска материалов на главной странице
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MaterialStudentMain(string Search)
        {
            try
            {
                if (Search != null)
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
                    List<Material> materials = new List<Material>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.GetAsync(apiUrl + "Materials/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);

                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";

                            return RedirectToAction("MainStudent", "Student");
                        }

                        TempData["SearchMaterial"] = JsonConvert.SerializeObject(materials);

                        return RedirectToAction("MainStudent", "Student");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";
                    return RedirectToAction("MainStudent", "Student");

                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }
        }
        /// <summary>
        /// POST запрос фильтрации материалов студента
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationMaterialStudent(int typeId, int disciplineID)
        {
            try
            {
                if (typeId != 0 || disciplineID != 0)
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

                        
                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("MaterialStudent", "Student");

                        }
                       
                        TempData["Filtration"] = JsonConvert.SerializeObject(materials);

                        return RedirectToAction("MaterialStudent", "Student");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";
                    return RedirectToAction("MaterialStudent", "Student");
                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }


        /// <summary>
        /// POST запрос фильтрации материалов на странице новостей
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationMaterialStudentNews(int typeId, int disciplineID)
        {
            try
            {
                if (typeId != 0 || disciplineID != 0)
                {
                    
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Material> materials = new List<Material>();

                    using (var httpClient = new HttpClient())
                    {

                        if (typeId != 0 && disciplineID == 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Materials/NoAuthFiltration?idType=" + typeId))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                            }
                        }
                        else if (typeId == 0 && disciplineID != 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Materials/NoAuthFiltration?idDiscipline=" + disciplineID))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                            }
                        }
                        else
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + $"Materials/NoAuthFiltration?idDiscipline={disciplineID}&idType={typeId}"))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                            }

                        }


                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("News", "Student");


                        }
                        TempData["FiltrationMaterial"] = JsonConvert.SerializeObject(materials);

                        return RedirectToAction("News", "Student");


                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("News", "Student");

                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }

        /// <summary>
        /// POST запрос фильтрации материалов на главной странице
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationMaterialStudentMain(int typeId, int disciplineID)
        {
            try
            {
                if (typeId != 0 || disciplineID != 0)
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


                        if (materials.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("MainStudent", "Student");



                        }

                        TempData["FiltrationMaterial"] = JsonConvert.SerializeObject(materials);

                        return RedirectToAction("MainStudent", "Student");



                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("MainStudent", "Student");


                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }

        /// <summary>
        /// POST запрос поиска тестирования
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestStudent(string Search)
        {
            try
            {
                if (Search != null)
                {
                   
                    var apiUrl = configuration["AppSettings:ApiUrl"];
                    List<Test> tests = new List<Test>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {

                        var response = await httpClient.GetAsync(apiUrl + "Tests/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                        
                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            if (!TempData.ContainsKey("AuthUser"))
                            {
                                return RedirectToAction("News", "Student");
                            }
                            else
                            {
                                return RedirectToAction("TestStudent", "Student");

                            }

                        }

                        if (!TempData.ContainsKey("AuthUser"))
                        {
                            TempData["SearchTest"] = JsonConvert.SerializeObject(tests);

                            return RedirectToAction("News", "Student");
                        }
                        else
                        {
                            TempData["Search"] = JsonConvert.SerializeObject(tests);

                            return RedirectToAction("TestStudent", "Student");
                        }
                        
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    if (!TempData.ContainsKey("AuthUser"))
                    {
                        return RedirectToAction("News", "Student");
                    }
                    else
                    {

                        return RedirectToAction("TestStudent", "Student");

                    }
                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }



        }

        /// <summary>
        /// POST запрос фильтрации тестирований
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationTestStudent(int parameterId, int disciplineID)
        {
            try
            {
                if (parameterId != 0 || disciplineID != 0)
                {
                    
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Test> tests = new List<Test>();

                    using (var httpClient = new HttpClient())
                    {

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
                       
                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            if (!TempData.ContainsKey("AuthUser"))
                            {
                                return RedirectToAction("News", "Student");
                            }
                            else if(TempData.ContainsKey("AuthUser"))
                            {
                                return RedirectToAction("MainStudent", "Student");

                            }
                            return RedirectToAction("TestStudent", "Student");

                        }

                        if (!TempData.ContainsKey("AuthUser"))
                        {
                            TempData["FiltrationTest"] = JsonConvert.SerializeObject(tests);

                            return RedirectToAction("News", "Student");
                        }
                        else if (TempData.ContainsKey("AuthUser"))
                        {
                            TempData["FiltrationTest"] = JsonConvert.SerializeObject(tests);

                            return RedirectToAction("MainStudent", "Student");

                        }
                        TempData["Filtration"] = JsonConvert.SerializeObject(tests);

                        return RedirectToAction("TestStudent", "Student");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    if (!TempData.ContainsKey("AuthUser"))
                    {
                        return RedirectToAction("News", "Student");
                    }
                    else if (TempData.ContainsKey("AuthUser"))
                    {

                        return RedirectToAction("MainStudent", "Student");

                    }
                    return RedirectToAction("TestStudent", "Student");
                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }


        /// <summary>
        /// POST запрос поиска тестирований на главной странице
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestStudentMain(string Search)
        {
            try
            {
                if (Search != null)
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
                    List<Test> tests = new List<Test>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.GetAsync(apiUrl + "Tests/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);

                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";

                            return RedirectToAction("MainStudent", "Student");


                        }
                        TempData["SearchTest"] = JsonConvert.SerializeObject(tests);

                        return RedirectToAction("MainStudent", "Student");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";
                    return RedirectToAction("MainStudent", "Student");

                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }



        }

        /// <summary>
        /// POST запрос фильтрации тестирований
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationTestStudentMain(int parameterId, int disciplineID)
        {
            try
            {
                if (parameterId != 0 || disciplineID != 0)
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

                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("MainStudent", "Student");
                        }


                        TempData["FiltrationTest"] = JsonConvert.SerializeObject(tests);
                        return RedirectToAction("MainStudent", "Student");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";
                    return RedirectToAction("MainStudent", "Student");
                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }

        /// <summary>
        /// POST запрос сохранения результатов тестирования
        /// </summary>
        /// <param name="userAnswers"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestSave(List<UserAnswerViewModel> userAnswers)
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
                int testID = int.Parse(TempData["testID"].ToString());
                TempData.Keep("testID");

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                StudentTest studentTest = new StudentTest();
                TestQuestion question = new TestQuestion();
                QuestionAnswerOption questionAnswerOption = new QuestionAnswerOption();

                studentTest.StudentId = id;
                studentTest.TestId = testID;
                studentTest.Result = 0;
                studentTest.IsCompleted = 0;
                studentTest.DateCompleted = DateTime.Now;

                StringContent content = new StringContent(JsonConvert.SerializeObject(studentTest), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


                    var response = await httpClient.PostAsync(apiUrl + "StudentTests", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        var createdStudentTest = JsonConvert.DeserializeObject<StudentTest>(responseContent);

                        for (int i = 0; i < userAnswers.Count; i++)
                        {
                            using (var responseQuestion = await httpClient.GetAsync(apiUrl + "TestQuestions/Search?search=" + int.Parse(userAnswers[i].QuestionId.ToString())))
                            {
                                string apiResponse = await responseQuestion.Content.ReadAsStringAsync();
                                List<TestQuestion> questions = JsonConvert.DeserializeObject<List<TestQuestion>>(apiResponse);
                                question = questions.FirstOrDefault();
                            }
                            StudentAnswer studentAnswer = new StudentAnswer();
                            if (userAnswers[i].QuestionTypeId == 1)
                            {
                                studentAnswer.StudentTestId = createdStudentTest.IdStudentTest;
                                studentAnswer.ContentAnswer = userAnswers[i].AnswerOption;
                                studentAnswer.TestQuestionId = question.IdTestQuestion;
                                studentAnswer.IsDeleted = 0;
                                StringContent contentStudentAnswer = new StringContent(JsonConvert.SerializeObject(studentAnswer), Encoding.UTF8, "application/json");

                                var responseStudentAnswer = await httpClient.PostAsync(apiUrl + "StudentAnswers", contentStudentAnswer);
                            }
                            else if (userAnswers[i].QuestionTypeId == 2)
                            {
                                studentAnswer.StudentTestId = createdStudentTest.IdStudentTest;
                                studentAnswer.TestQuestionId = question.IdTestQuestion;
                                studentAnswer.IsDeleted = 0;
                                using (var responseAnswer = await httpClient.GetAsync(apiUrl + "QuestionAnswerOptions/FiltrationAnswer?idAnswer=" + int.Parse(userAnswers[i].SelectedAnswerId.ToString())))
                                {
                                    string apiResponse = await responseAnswer.Content.ReadAsStringAsync();

                                    List<QuestionAnswerOption> questionAnswerOptions = JsonConvert.DeserializeObject<List<QuestionAnswerOption>>(apiResponse);
                                    questionAnswerOption = questionAnswerOptions.FirstOrDefault();
                                }
                                studentAnswer.QuestionAnswerOptionsId = questionAnswerOption.IdQuestionAnswerOptions;
                                StringContent contentStudentAnswer = new StringContent(JsonConvert.SerializeObject(studentAnswer), Encoding.UTF8, "application/json");

                                var responseStudentAnswer = await httpClient.PostAsync(apiUrl + "StudentAnswers", contentStudentAnswer);
                            }
                            else if (userAnswers[i].QuestionTypeId == 3)
                            {
                                foreach(var item in userAnswers[i].SelectedAnswerIds)
                                {
                                    studentAnswer.StudentTestId = createdStudentTest.IdStudentTest;
                                    studentAnswer.TestQuestionId = question.IdTestQuestion;
                                    studentAnswer.IsDeleted = 0;
                                    using (var responseAnswer = await httpClient.GetAsync(apiUrl + "QuestionAnswerOptions/FiltrationAnswer?idAnswer=" + item))
                                    {
                                        string apiResponse = await responseAnswer.Content.ReadAsStringAsync();
                                        List<QuestionAnswerOption> questionAnswerOptions = JsonConvert.DeserializeObject<List<QuestionAnswerOption>>(apiResponse);
                                        questionAnswerOption = questionAnswerOptions.FirstOrDefault();
                                    }

                                    studentAnswer.QuestionAnswerOptionsId = questionAnswerOption.IdQuestionAnswerOptions;

                                    StringContent contentAnswerOption = new StringContent(JsonConvert.SerializeObject(studentAnswer), Encoding.UTF8, "application/json");
                                    var responseAnswerOption = await httpClient.PostAsync(apiUrl + "StudentAnswers", contentAnswerOption);
                                }
                            }
                            else
                            {
                                return BadRequest("Ошибка!");
                            }
                        }
                    }
                    else
                    {
                        return BadRequest();

                    } 
                    return RedirectToAction("TestStudent", "Student");
                }

            }catch (Exception e)
            {
                return BadRequest($"Failed to save {e}");
            }
        }



        /// <summary>
        /// Загрузка главной страницы студента
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> MainStudent()
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

                int id = int.Parse(TempData["AuthUser"].ToString());
                TempData.Keep("AuthUser");
                Student student = new Student();
                List<StudyGroup> studyGroups = null;
                List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups = new List<DisciplineOfTheStudyGroup>();
                List<Employee> employees = new List<Employee>();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();
                List<Test> tests = new List<Test>();
                List<Material> materials = new List<Material>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
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
                    if (TempData.ContainsKey("SearchTest"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["SearchTest"].ToString());
                    }
                    else if (TempData.ContainsKey("FiltrationTest"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["FiltrationTest"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Tests"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                        }
                    }
                    if (TempData.ContainsKey("SearchMaterial"))
                    {
                        materials = JsonConvert.DeserializeObject<List<Material>>(TempData["SearchMaterial"].ToString());
                    }
                    else if (TempData.ContainsKey("FiltrationMaterial"))
                    {
                        materials = JsonConvert.DeserializeObject<List<Material>>(TempData["FiltrationMaterial"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Materials"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                        }
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                    }
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
                    var group = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId).IdStudyGroup;
                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineOfTheStudyGroups/Filtration?idStudyGroup=" + group))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineOfTheStudyGroups = JsonConvert.DeserializeObject<List<DisciplineOfTheStudyGroup>>(apiResponse);
                    }
                }

                    
                    
                    StudyGroup studyGroup = studyGroups.Find(n => n.IdStudyGroup == student.StudyGroupId);
                    NewsModel newsModel = new NewsModel(employees, disciplines, disciplineEmployees, testParameters, tests, student, disciplineOfTheStudyGroups, studyGroup, materials, typeMaterials);
                    return View(newsModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Загрузка главной страницы
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> News()
        {
            try
            {
                TempData.Remove("AuthUser");
                var apiUrl = configuration["AppSettings:ApiUrl"];


                List<Discipline> disciplines = new List<Discipline>();
                List<Test> tests = new List<Test>();
                List<Material> materials = new List<Material>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();
                using (var httpClient = new HttpClient())
                {
                    
                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    if (TempData.ContainsKey("SearchTest"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["SearchTest"].ToString());
                    }
                    else if (TempData.ContainsKey("FiltrationTest"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["FiltrationTest"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Tests"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                        }
                    }
                    if (TempData.ContainsKey("SearchMaterial"))
                    {
                        materials = JsonConvert.DeserializeObject<List<Material>>(TempData["SearchMaterial"].ToString());
                    }
                    else if (TempData.ContainsKey("FiltrationMaterial"))
                    {
                        materials = JsonConvert.DeserializeObject<List<Material>>(TempData["FiltrationMaterial"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Materials/NoAuth"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                        }
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                    }
                }

                NewsModel newsModel = new NewsModel(null, disciplines, null, null, tests, null, null, null, materials, typeMaterials);
                return View(newsModel);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы пройденных тестирований
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> MyTest()
        {
            try
            {
                if (!TempData.ContainsKey("AuthUser"))
                {
                    return RedirectToAction("Authorization", "Home");
                }
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

                int id = int.Parse(TempData["AuthUser"].ToString());
                TempData.Keep("AuthUser");
                Student student = new Student();
                List<Discipline> disciplines = new List<Discipline>();  
                List<Test> tests = new List<Test>();
                List<StudentTest> studentTest = new List<StudentTest>();
                List<TestQuestion> question = new List<TestQuestion>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Students/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        student = JsonConvert.DeserializeObject<Student>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Tests"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "StudentTests"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studentTest = JsonConvert.DeserializeObject<List<StudentTest>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "TestQuestions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        question = JsonConvert.DeserializeObject<List<TestQuestion>>(apiResponse);
                    }
                }

                List<StudentTest> studentTests = studentTest.Where(n => n.StudentId == student.IdStudent).ToList();

                MyTestModelView myTestModelView = new MyTestModelView(student, disciplines, tests, studentTests, question);

                return View(myTestModelView);
                

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы ответов студента на тестирование
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> MyTestAnswer(int id)
        {
            try
            {
                if (!TempData.ContainsKey("AuthUser"))
                {
                    return RedirectToAction("Authorization", "Home");
                }
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

                int userId = int.Parse(TempData["AuthUser"].ToString());
                TempData.Keep("AuthUser");
                Test test = new Test();
                List<StudentAnswer> studentAnswers = new List<StudentAnswer>();
                List<StudentTest> studentTest = new List<StudentTest>();
                Student student = new Student();
                List<Discipline> disciplines = new List<Discipline>();
                List<TestQuestion> testQuestion = new List<TestQuestion>();
                List<Question> questions = new List<Question>();    
                List<AnswerOption> answerOptions = new List<AnswerOption>();
                List<QuestionAnswerOption> questionAnswerOptions = new List<QuestionAnswerOption>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Tests/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        test = JsonConvert.DeserializeObject<Test>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "StudentAnswers"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studentAnswers = JsonConvert.DeserializeObject<List<StudentAnswer>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "StudentTests"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studentTest = JsonConvert.DeserializeObject<List<StudentTest>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Students/" + userId))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        student = JsonConvert.DeserializeObject<Student>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "TestQuestions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testQuestion = JsonConvert.DeserializeObject<List<TestQuestion>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Questions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        questions = JsonConvert.DeserializeObject<List<Question>>(apiResponse);
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
                }

                List<StudentTest> studentTests = studentTest.Where(n => n.StudentId == student.IdStudent).ToList();
                List<StudentAnswer> studentAnswer = studentAnswers.Where(n => n.StudentTestId == studentTests.FirstOrDefault(st => st.TestId == test.IdTest && st.StudentId == student.IdStudent)?.IdStudentTest).ToList();

                MyTestAnswerModelView myTestAnswerModelView = new MyTestAnswerModelView(test, studentAnswer, studentTests, student, disciplines, testQuestion, questions, answerOptions, questionAnswerOptions);
                return View(myTestAnswerModelView);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
