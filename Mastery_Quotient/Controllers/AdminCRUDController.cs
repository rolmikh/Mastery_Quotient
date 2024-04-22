using FluentValidation;
using Google.Apis.Drive.v3.Data;
using Mastery_Quotient.Models;
using Mastery_Quotient.ModelsValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Mastery_Quotient.Controllers
{
    public class AdminCRUDController : Controller
    {
        private readonly IConfiguration configuration;

        private readonly IValidator<TypeMaterial> typeMaterialValidator;

        private readonly IValidator<TestParameter> testParameterValidator;

        private readonly IValidator<StudyGroup> studyGroupValidator;

        private readonly IValidator<Discipline> disciplineValidator;

        public AdminCRUDController(IConfiguration configuration, IValidator<TypeMaterial> typeMaterialValidator, IValidator<TestParameter> testParameterValidator, IValidator<StudyGroup> studyGroupValidator, IValidator<Discipline> disciplineValidator)
        {
            this.configuration = configuration;
            this.typeMaterialValidator = typeMaterialValidator;
            this.testParameterValidator = testParameterValidator;
            this.studyGroupValidator = studyGroupValidator;
            this.disciplineValidator = disciplineValidator;
        }

        public IActionResult AdminPanel()
        {
            return View();
        }

       /// <summary>
       /// Метод создания резервной копии базы данных
       /// </summary>
       /// <returns></returns>
        public async Task<IActionResult> BackUpDatabase()
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "BackUp/BackUp"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        TempData["Message"] = "Резервная копия базы данных успешно создана.";
                        return RedirectToAction("AdminPanel", "AdminCRUD");

                    }

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Загрузка представления страницы с параметрами тестирования
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TestParameterAdminPanel()
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<TestParameter> testParameters = new List<TestParameter>();

                using (var client = new HttpClient())
                {
                    if (TempData.ContainsKey("Search"))
                    {
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(TempData["Search"].ToString());
                    }
                    else
                    {
                        using (var response = await client.GetAsync(apiUrl + "TestParameters/NoDeleted"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                        }
                    }
                }

                TestParameterView testParameter = new TestParameterView(testParameters);
                return View(testParameter);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка" + e.Message);
            }

        }

        /// <summary>
        /// POST запрос поиска параметров тестирования
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SearchTestParameter(string Search)
        {
            if (Search != null)
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<TestParameter> testParameters = new List<TestParameter>();


                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl + "TestParameters/Search?search=" + Search);

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);

                    TempData["Search"] = JsonConvert.SerializeObject(testParameters);

                    if (testParameters.Count == 0)
                    {
                        TempData["Message"] = "По вашему запросу ничего не найдено";
                        return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");

                    }

                    return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
            }
        }

        /// <summary>
        /// POST запрос добавления параметров тестирования
        /// </summary>
        /// <param name="nameParameter"></param>
        /// <param name="valueParameter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestParameterNew(string nameParameter, string valueParameter)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                TestParameter testParameter = new TestParameter();

                testParameter.NameParameter = nameParameter;
                testParameter.ValueParameter = valueParameter;
                testParameter.IsDeleted = 0;

                var validationResult = await testParameterValidator.ValidateAsync(testParameter);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(testParameter), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "TestParameters", content);

                    return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных" + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTestParameter(int id)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                TestParameter testParameter = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameter = JsonConvert.DeserializeObject<TestParameter>(apiResponse);
                    }
                }

                TestParameterView testParameterModel = new TestParameterView(new List<TestParameter> { testParameter });
                return View(testParameterModel);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка" + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на изменение данных параметра тестирования
        /// </summary>
        /// <param name="idParameter"></param>
        /// <param name="nameParameter"></param>
        /// <param name="valueParameter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTestParameter(int idParameter, string nameParameter, string valueParameter)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                TestParameter testParameter = new TestParameter();

                testParameter.IdTestParameter = idParameter;
                testParameter.NameParameter = nameParameter;
                testParameter.ValueParameter = valueParameter;
                testParameter.IsDeleted = 0;

                var validationResult = await testParameterValidator.ValidateAsync(testParameter);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(testParameter), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PutAsync(apiUrl + "TestParameters/" + idParameter, content);

                    return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на удаление данных
        /// </summary>
        /// <param name="IdTestParameter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTestParameter(int IdTestParameter)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.DeleteAsync(apiUrl + "TestParameters/" + IdTestParameter))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы с типами материала
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TypeMaterialAdminPanel()
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();

                using (var client = new HttpClient())
                {
                    if (TempData.ContainsKey("Search"))
                    {
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(TempData["Search"].ToString());
                    }
                    else
                    {
                        using (var response = await client.GetAsync(apiUrl + "TypeMaterials/NoDeleted"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                        }
                    }
                }

                TypeMaterialView typeMaterial = new TypeMaterialView(typeMaterials);
                return View(typeMaterial);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка" + e.Message);
            }

        }

        /// <summary>
        /// POST запрос поиска типов материала
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SearchTypeMaterial(string Search)
        {
            if (Search != null)
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();


                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/Search?search=" + Search);

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);

                    TempData["Search"] = JsonConvert.SerializeObject(typeMaterials);

                    if (typeMaterials.Count == 0)
                    {
                        TempData["Message"] = "По вашему запросу ничего не найдено";
                        return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");

                    }

                    return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
            }
        }

        /// <summary>
        /// POST запрос на добавление типа материала
        /// </summary>
        /// <param name="nameParameter"></param>
        /// <param name="valueParameter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TypeMaterialNew(string nameTypeMaterial)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                TypeMaterial typeMaterial = new TypeMaterial();

                typeMaterial.NameTypeMaterial = nameTypeMaterial;
                typeMaterial.IsDeleted = 0;

                var validationResult = await typeMaterialValidator.ValidateAsync(typeMaterial);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(typeMaterial), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "TypeMaterials", content);

                    return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных" + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTypeMaterial(int id)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                TypeMaterial typeMaterial = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterial = JsonConvert.DeserializeObject<TypeMaterial>(apiResponse);
                    }
                }
                TypeMaterialView typeMaterialView = new TypeMaterialView(new List<TypeMaterial> { typeMaterial});
                return View(typeMaterialView);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка" + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на изменение данных о типе материала
        /// </summary>
        /// <param name="IdTypeMaterial"></param>
        /// <param name="NameTypeMaterial"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTypeMaterial(int IdTypeMaterial, string NameTypeMaterial)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                TypeMaterial typeMaterial = new TypeMaterial();
                typeMaterial.IdTypeMaterial = IdTypeMaterial;
                typeMaterial.NameTypeMaterial = NameTypeMaterial;
                typeMaterial.IsDeleted = 0;

                var validationResult = await typeMaterialValidator.ValidateAsync(typeMaterial);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(typeMaterial), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PutAsync(apiUrl + "TypeMaterials/" + IdTypeMaterial, content);

                    return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на удаление данных
        /// </summary>
        /// <param name="IdTypeMaterial"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTypeMaterial(int IdTypeMaterial)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.DeleteAsync(apiUrl + "TypeMaterials/" + IdTypeMaterial))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }




        /// <summary>
        /// Загрузка представления страницы учебных групп
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> StudyGroupAdminPanel()
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<Course> courses = new List<Course>();

                using (var client = new HttpClient())
                {
                    if (TempData.ContainsKey("Search"))
                    {
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(TempData["Search"].ToString());
                    }
                    else if (TempData.ContainsKey("Filtration"))
                    {
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(TempData["Filtration"].ToString());

                    }
                    else
                    {
                        using (var response = await client.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                        }
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }

                }

                GroupModel groupModel = new GroupModel(studyGroups, courses);
                return View(groupModel);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка" + e.Message);
            }

        }

        /// <summary>
        /// POST запрос поиска учебной группы
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SearchStudyGroup(string Search)
        {
            if (Search != null)
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<StudyGroup> studyGroups = new List<StudyGroup>();


                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl + "StudyGroups/Search?search=" + Search);

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);

                    TempData["Search"] = JsonConvert.SerializeObject(studyGroups);

                    if (studyGroups.Count == 0)
                    {
                        TempData["Message"] = "По вашему запросу ничего не найдено";
                        return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");

                    }

                    return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
            }
        }

        /// <summary>
        /// POST запрос добавления учебной группы
        /// </summary>
        /// <param name="nameStudyGroup"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> StudyGroupNew(string nameStudyGroup, int courseId)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                StudyGroup studyGroup = new StudyGroup();
                studyGroup.NameStudyGroup = nameStudyGroup;
                studyGroup.CourseId = courseId;
                studyGroup.IsDeleted = 0;

                var validationResult = await studyGroupValidator.ValidateAsync(studyGroup);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(studyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "StudyGroups", content);

                    return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateStudyGroup(int id)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                StudyGroup studyGroup = null;
                List<Course> courses = new List<Course>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroup = JsonConvert.DeserializeObject<StudyGroup>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                }
                GroupModel groupModel = new GroupModel(new List<StudyGroup> { studyGroup}, courses);
                return View(groupModel);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на обновление данных о учебной группе
        /// </summary>
        /// <param name="idStudyGroup"></param>
        /// <param name="nameStudyGroup"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateStudyGroup(int idStudyGroup, string nameStudyGroup, int courseId)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                StudyGroup studyGroup = new StudyGroup();

                studyGroup.IdStudyGroup = idStudyGroup;
                studyGroup.NameStudyGroup = nameStudyGroup;
                studyGroup.CourseId = courseId;
                studyGroup.IsDeleted = 0;

                var validationResult = await studyGroupValidator.ValidateAsync(studyGroup);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateStudyGroup", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(studyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PutAsync(apiUrl + "StudyGroups/" + idStudyGroup, content);

                    return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

       /// <summary>
       /// POST запрос удаления данных о учебной группе
       /// </summary>
       /// <param name="IdStudyGroup"></param>
       /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteStudyGroup(int IdStudyGroup)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.DeleteAsync(apiUrl + "StudyGroups/" + IdStudyGroup))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }

        /// <summary>
        /// POST запрос фильтрации учебных групп
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationStudyGroup(int courseId)
        {

            if (courseId != 0)
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<StudyGroup> studyGroups = new List<StudyGroup>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "StudyGroups/Filtration?idCourse=" + courseId))
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                        TempData["Filtration"] = JsonConvert.SerializeObject(studyGroups);

                        if (studyGroups.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");

                        }

                        return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
                    }

                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
            }

        }



        /// <summary>
        /// Загрузка представления страницы дисциплины
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DisciplineAdminPanel()
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Discipline> disciplines = new List<Discipline>();
                List<Course> courses = new List<Course>();

                using (var client = new HttpClient())
                {
                    if (TempData.ContainsKey("Search"))
                    {
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(TempData["Search"].ToString());
                    }
                    else if (TempData.ContainsKey("Filtration"))
                    {
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(TempData["Filtration"].ToString());

                    }
                    else
                    {
                        using (var response = await client.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                        }
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }

                }
                DisciplineModelView disciplineModelView = new DisciplineModelView(disciplines, courses);
                return View(disciplineModelView);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка " + e.Message);
            }

        }

        /// <summary>
        /// POST запрос поиска дисциплины
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SearchDiscipline(string Search)
        {
            if (Search != null)
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<Discipline> disciplines = new List<Discipline>();


                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl + "Disciplines/Search?search=" + Search);

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);

                    TempData["Search"] = JsonConvert.SerializeObject(disciplines);

                    if (disciplines.Count == 0)
                    {
                        TempData["Message"] = "По вашему запросу ничего не найдено";
                        return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");

                    }

                    return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
            }
        }

        /// <summary>
        /// POST запрос добавления дисциплины
        /// </summary>
        /// <param name="nameDiscipline"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DisciplineNew(string nameDiscipline, int courseId)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Discipline discipline = new Discipline();
                discipline.NameDiscipline = nameDiscipline;
                discipline.CourseId = courseId;
                discipline.IsDeleted = 0;

                var validationResult = await disciplineValidator.ValidateAsync(discipline);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(discipline), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Disciplines", content);

                    return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateDiscipline(int id)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Discipline discipline = null;
                List<Course> courses = new List<Course>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        discipline = JsonConvert.DeserializeObject<Discipline>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                }
                DisciplineModelView disciplineModelView = new DisciplineModelView(new List<Discipline> { discipline}, courses);
                return View(disciplineModelView);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на изменение данных о дисциплине
        /// </summary>
        /// <param name="idDiscipline"></param>
        /// <param name="nameDiscipline"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateDiscipline(int idDiscipline, string nameDiscipline, int courseId)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                Discipline discipline = new Discipline();
                discipline.IdDiscipline = idDiscipline;
                discipline.NameDiscipline = nameDiscipline;
                discipline.CourseId = courseId;
                discipline.IsDeleted = 0;


                var validationResult = await disciplineValidator.ValidateAsync(discipline);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateDiscipline", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(discipline), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PutAsync(apiUrl + "Disciplines/" + idDiscipline, content);

                    return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на удаление данных о дисциплине
        /// </summary>
        /// <param name="IdDiscipline"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteDiscipline(int IdDiscipline)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.DeleteAsync(apiUrl + "Disciplines/" + IdDiscipline))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }

        /// <summary>
        /// POST запрос фильтрации дисциплин
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationDiscipline(int courseId)
        {

            if (courseId != 0)
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                List<Discipline> disciplines = new List<Discipline>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/Filtration?idCourse=" + courseId))
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                        TempData["Filtration"] = JsonConvert.SerializeObject(disciplines);

                        if (disciplines.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");

                        }

                        return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
                    }

                }
            }
            else
            {
                TempData["Message"] = "По вашему запросу ничего не найдено";

                return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
            }

        }
    }
}
