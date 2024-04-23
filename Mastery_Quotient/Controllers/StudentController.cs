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

namespace Mastery_Quotient.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<StudentController> _logger;

        private readonly IValidator<Student> studentValidator;

        FirebaseService firebaseService = new FirebaseService();

        public StudentController(IConfiguration configuration, ILogger<StudentController> logger, IValidator<Student> studentValidator)
        {
            this.configuration = configuration;
            _logger = logger;
            this.studentValidator = studentValidator;
        }

        public IActionResult MainStudent()
        {
            return View();
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
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Student student = new Student();
                List<StudyGroup> studyGroups = new List<StudyGroup>();

                using (var httpClient = new HttpClient())
                {
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

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl + "Students/" + id);
                    response.EnsureSuccessStatusCode();
                    var studentData = await response.Content.ReadAsStringAsync();

                    var student = JsonConvert.DeserializeObject<Student>(studentData);

                    student.SurnameStudent = surnameUser;
                    student.NameStudent = nameUser;
                    student.EmailStudent = emailUser;
                    student.MiddleNameStudent = middleNameUser;

                    var validationResult = await studentValidator.ValidateAsync(student);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
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


            return RedirectToAction("Authorization", "Home");
        }

        /// <summary>
        /// Загрузка представления страницы материалов студента
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MaterialStudent()
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Material> materials = new List<Material>();
                List<Discipline> disciplines = new List<Discipline>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();
                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups = new List<DisciplineOfTheStudyGroup>();
                List<Employee> employees = new List<Employee>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                Student student = new Student();

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                using (var httpClient = new HttpClient()) 
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Materials"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
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
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees"))
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
                //if (TempData.ContainsKey("Search"))
                //{
                //    materials = JsonConvert.DeserializeObject<List<Material>>(TempData["Search"].ToString());
                //}
                //else if (TempData.ContainsKey("Filtration"))
                //{
                //    materials = JsonConvert.DeserializeObject<List<Material>>(TempData["Filtration"].ToString());
                //}
                //else
                //{
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

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
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
                    using (var response = await httpClient.GetAsync(apiUrl + "StudentTest"))
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

                TempData["testID"] = testId;
                TempData.Keep("testID");


                int testID = int.Parse(TempData["testID"].ToString());
                TempData.Keep("testID");


                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

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

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
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
                List<TestParameter> testsParameters = testParameters.Where(n => n.IdTestParameter == test.TestParameterId).ToList();
                List<TestQuestion> testQuestionsList = testQuestions.Where(n => n.TestId == test.IdTest).ToList();

                ViewTestModel viewTestModel = new ViewTestModel(test, typeQuestions, testsParameters, questions, testQuestionsList, answerOptions, questionAnswerOptions, employee, disciplines, discipline);

                return View(viewTestModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
