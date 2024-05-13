using FluentValidation;
using Mastery_Quotient.Models;
using Mastery_Quotient.ModelsValidation;
using Mastery_Quotient.Service;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Mastery_Quotient.Controllers
{
    public class AdminCRUDController : Controller
    {
        private readonly IConfiguration configuration;

        private readonly IValidator<TypeMaterial> typeMaterialValidator;

        private readonly IValidator<TestParameter> testParameterValidator;

        private readonly IValidator<StudyGroup> studyGroupValidator;

        private readonly IValidator<Discipline> disciplineValidator;

        private readonly IValidator<DisciplineEmployee> disciplineEmployeeValidator;

        private readonly IValidator<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroupValidator;

        private readonly IValidator<EmployeeStudyGroup> employeeStudyGroupValidator;

        TokenService tokenService = new TokenService();

        public AdminCRUDController(IConfiguration configuration, IValidator<TypeMaterial> typeMaterialValidator, IValidator<TestParameter> testParameterValidator, IValidator<StudyGroup> studyGroupValidator, IValidator<Discipline> disciplineValidator, IValidator<DisciplineEmployee> disciplineEmployeeValidator, IValidator<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroupValidator, IValidator<EmployeeStudyGroup> employeeStudyGroupValidator)
        {
            this.configuration = configuration;
            this.typeMaterialValidator = typeMaterialValidator;
            this.testParameterValidator = testParameterValidator;
            this.studyGroupValidator = studyGroupValidator;
            this.disciplineValidator = disciplineValidator;
            this.disciplineEmployeeValidator = disciplineEmployeeValidator;
            this.disciplineOfTheStudyGroupValidator = disciplineOfTheStudyGroupValidator;
            this.employeeStudyGroupValidator = employeeStudyGroupValidator;
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

        public async Task<IActionResult> SQLScript()
        {
            string connectionString = "Data Source=ROLMIKH;Initial Catalog=Mastery_Quotient_Database;Persist Security Info=True;User ID=sa;Password=1505;Encrypt=True;";

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                ServerConnection serverConnection = new ServerConnection(builder.DataSource, builder.UserID, builder.Password);
                Server server = new Server(serverConnection);
                Database database = server.Databases[builder.InitialCatalog];

                ScriptingOptions options = new ScriptingOptions();
                options.ScriptData = true;
                options.ScriptSchema = true;
                options.EnforceScriptingOptions = true;

                StringBuilder script = new StringBuilder();
                foreach (Table table in database.Tables)
                {
                    IEnumerable<string> tableScripts = table.EnumScript(options);
                    foreach (string tableScript in tableScripts)
                    {
                        script.AppendLine(tableScript);
                    }
                }

                string outputFileName = "DatabaseScript.sql";
                byte[] fileBytes = Encoding.UTF8.GetBytes(script.ToString());

                return File(fileBytes, "application/force-download", outputFileName);
            }
            catch (Exception ex)
            {
                // Log the error and return an error message or view
                return Content("Error: " + ex.Message);
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

                List<TestParameter> testParameters = new List<TestParameter>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                List<TestParameter> testParameters = new List<TestParameter>();


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                TestParameter testParameter = new TestParameter();

                testParameter.NameParameter = nameParameter;
                testParameter.ValueParameter = valueParameter;
                testParameter.IsDeleted = 0;
                try
                {
                    var validationResult = await testParameterValidator.ValidateAsync(testParameter);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
                    }
                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TestParameterAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(testParameter), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                TestParameter testParameter = null;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                TypeMaterial typeMaterial = new TypeMaterial();

                typeMaterial.NameTypeMaterial = nameTypeMaterial;
                typeMaterial.IsDeleted = 0;

                
                try
                {
                    var validationResult = await typeMaterialValidator.ValidateAsync(typeMaterial);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TypeMaterialAdminPanel", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(typeMaterial), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                TypeMaterial typeMaterial = null;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterial = JsonConvert.DeserializeObject<TypeMaterial>(apiResponse);
                    }
                }
                TypeMaterialView typeMaterialView = new TypeMaterialView(new List<TypeMaterial> { typeMaterial });
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

                TypeMaterial typeMaterial = new TypeMaterial();
                typeMaterial.IdTypeMaterial = IdTypeMaterial;
                typeMaterial.NameTypeMaterial = NameTypeMaterial;
                typeMaterial.IsDeleted = 0;

                try
                {
                    var validationResult = await typeMaterialValidator.ValidateAsync(typeMaterial);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateTypeMaterial", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateTypeMaterial", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(typeMaterial), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<Course> courses = new List<Course>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                List<StudyGroup> studyGroups = new List<StudyGroup>();


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                StudyGroup studyGroup = new StudyGroup();
                studyGroup.NameStudyGroup = nameStudyGroup;
                studyGroup.CourseId = courseId;
                studyGroup.IsDeleted = 0;

               
                try
                {
                    var validationResult = await studyGroupValidator.ValidateAsync(studyGroup);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("StudyGroupAdminPanel", "AdminCRUD");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(studyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                StudyGroup studyGroup = null;
                List<Course> courses = new List<Course>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                GroupModel groupModel = new GroupModel(new List<StudyGroup> { studyGroup }, courses);
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

                StudyGroup studyGroup = new StudyGroup();

                studyGroup.IdStudyGroup = idStudyGroup;
                studyGroup.NameStudyGroup = nameStudyGroup;
                studyGroup.CourseId = courseId;
                studyGroup.IsDeleted = 0;

                
                try
                {
                    var validationResult = await studyGroupValidator.ValidateAsync(studyGroup);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateStudyGroup", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateStudyGroup", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(studyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                List<StudyGroup> studyGroups = new List<StudyGroup>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                List<Discipline> disciplines = new List<Discipline>();
                List<Course> courses = new List<Course>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                List<Discipline> disciplines = new List<Discipline>();


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                Discipline discipline = new Discipline();
                discipline.NameDiscipline = nameDiscipline;
                discipline.CourseId = courseId;
                discipline.IsDeleted = 0;

                try
                {
                    var validationResult = await disciplineValidator.ValidateAsync(discipline);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("DisciplineAdminPanel", "AdminCRUD");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(discipline), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                Discipline discipline = null;
                List<Course> courses = new List<Course>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
                DisciplineModelView disciplineModelView = new DisciplineModelView(new List<Discipline> { discipline }, courses);
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

                Discipline discipline = new Discipline();
                discipline.IdDiscipline = idDiscipline;
                discipline.NameDiscipline = nameDiscipline;
                discipline.CourseId = courseId;
                discipline.IsDeleted = 0;

                try
                {
                    var validationResult = await disciplineValidator.ValidateAsync(discipline);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateDiscipline", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateDiscipline", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(discipline), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                List<Discipline> disciplines = new List<Discipline>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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


        /// <summary>
        /// Загрузка представления учебных групп преподавателей  
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> StudyGroupTeacher()
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


                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<Course> courses = new List<Course>();
                List<EmployeeStudyGroup> employeeStudyGroups = new List<EmployeeStudyGroup>();
                List<Employee> employees = new List<Employee>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await client.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "EmployeeStudyGroups"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employeeStudyGroups = JsonConvert.DeserializeObject<List<EmployeeStudyGroup>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                    }


                }
                TeacherStudyGroup teacherStudyGroup = new TeacherStudyGroup(studyGroups, courses, employeeStudyGroups, employees);
                return View(teacherStudyGroup);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка " + e.Message);
            }

        }


        /// <summary>
        /// POST запрос на добавление учебной группы сотрудника
        /// </summary>
        /// <param name="StudyGroupId"></param>
        /// <param name="EmployeeId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> StudyGroupEmployeeNew(int StudyGroupId, int EmployeeId)
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

                EmployeeStudyGroup employeeStudyGroup = new EmployeeStudyGroup();
                employeeStudyGroup.StudyGroupId = StudyGroupId;
                employeeStudyGroup.EmployeeId = EmployeeId;
                
                try
                {
                    var validationResult = await employeeStudyGroupValidator.ValidateAsync(employeeStudyGroup);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("StudyGroupTeacher", "AdminCRUD");

                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("StudyGroupTeacher", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(employeeStudyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PostAsync(apiUrl + "EmployeeStudyGroups", content);

                    return RedirectToAction("StudyGroupTeacher", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных о учебной группе преподавателя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateStudyGroupTeacher(int id)
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

                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<Course> courses = new List<Course>();
                EmployeeStudyGroup employeeStudyGroups = null;
                List<Employee> employees = new List<Employee>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await client.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "EmployeeStudyGroups/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employeeStudyGroups = JsonConvert.DeserializeObject<EmployeeStudyGroup>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                    }


                }
                TeacherStudyGroup teacherStudyGroup = new TeacherStudyGroup(studyGroups, courses, new List<EmployeeStudyGroup> { employeeStudyGroups }, employees);
                return View(teacherStudyGroup);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на изменение данных о учебной группе преподавателя
        /// </summary>
        /// <param name="IdEmployeeStudyGroup"></param>
        /// <param name="StudyGroupId"></param>
        /// <param name="EmployeeId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateStudyGroupTeacher(int IdEmployeeStudyGroup, int StudyGroupId, int EmployeeId)
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

                EmployeeStudyGroup employeeStudyGroup = new EmployeeStudyGroup();
                employeeStudyGroup.IdEmployeeStudyGroup = IdEmployeeStudyGroup;
                employeeStudyGroup.StudyGroupId = StudyGroupId;
                employeeStudyGroup.EmployeeId = EmployeeId;

                try
                {
                    var validationResult = await employeeStudyGroupValidator.ValidateAsync(employeeStudyGroup);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateStudyGroupTeacher", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateStudyGroupTeacher", "AdminCRUD");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(employeeStudyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PutAsync(apiUrl + "EmployeeStudyGroups/" + IdEmployeeStudyGroup, content);

                    return RedirectToAction("StudyGroupTeacher", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на удаление данных о учебной группе преподавателя
        /// </summary>
        /// <param name="IdEmployeeStudyGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteStudyGroupTeacher(int IdEmployeeStudyGroup)
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

                    using (var response = await httpClient.DeleteAsync(apiUrl + "EmployeeStudyGroups/" + IdEmployeeStudyGroup))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("StudyGroupTeacher", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы дисциплины учебной группы
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DisciplineStudyGroup()
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

                List<StudyGroup> studyGroups = new List<StudyGroup>();
                List<DisciplineOfTheStudyGroup> disciplineOfTheStudyGroups = new List<DisciplineOfTheStudyGroup>();
                List<Course> courses = new List<Course>();
                List<Discipline> disciplines = new List<Discipline>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await client.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "DisciplineOfTheStudyGroups"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineOfTheStudyGroups = JsonConvert.DeserializeObject<List<DisciplineOfTheStudyGroup>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }



                }
                DisciplineStudyGroupView disciplineStudyGroupView = new DisciplineStudyGroupView(studyGroups, disciplineOfTheStudyGroups, courses, disciplines);
                return View(disciplineStudyGroupView);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка " + e.Message);
            }

        }


        /// <summary>
        /// POST запрос на добавление дисциплины учебной группы
        /// </summary>
        /// <param name="StudyGroupId"></param>
        /// <param name="DisciplineId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> StudyGroupDisciplineNew(int StudyGroupId, int DisciplineId)
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

                DisciplineOfTheStudyGroup disciplineStudyGroup = new DisciplineOfTheStudyGroup();
                disciplineStudyGroup.StudyGroupId = StudyGroupId;
                disciplineStudyGroup.DisciplineId = DisciplineId;

                try
                {
                    var validationResult = await disciplineOfTheStudyGroupValidator.ValidateAsync(disciplineStudyGroup);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("DisciplineStudyGroup", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("DisciplineStudyGroup", "AdminCRUD");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(disciplineStudyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PostAsync(apiUrl + "DisciplineOfTheStudyGroups", content);

                    return RedirectToAction("DisciplineStudyGroup", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных о дисциплине учебной группы
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateDisciplineStudyGroup(int id)
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

                List<StudyGroup> studyGroups = new List<StudyGroup>();
                DisciplineOfTheStudyGroup disciplineOfTheStudyGroups = null;
                List<Course> courses = new List<Course>();
                List<Discipline> disciplines = new List<Discipline>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await client.GetAsync(apiUrl + "StudyGroups/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        studyGroups = JsonConvert.DeserializeObject<List<StudyGroup>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "DisciplineOfTheStudyGroups/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineOfTheStudyGroups = JsonConvert.DeserializeObject<DisciplineOfTheStudyGroup>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }


                }
                DisciplineStudyGroupView disciplineStudyGroupView = new DisciplineStudyGroupView(studyGroups, new List<DisciplineOfTheStudyGroup> { disciplineOfTheStudyGroups }, courses, disciplines);

                return View(disciplineStudyGroupView);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на изменение данных о дисциплины учебной группы
        /// </summary>
        /// <param name="IdDisciplineOfTheStudyGroup"></param>
        /// <param name="StudyGroupId"></param>
        /// <param name="DisciplineId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateDisciplineStudyGroup(int IdDisciplineOfTheStudyGroup, int StudyGroupId, int DisciplineId)
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

                DisciplineOfTheStudyGroup disciplineOfTheStudyGroup = new DisciplineOfTheStudyGroup();
                disciplineOfTheStudyGroup.IdDisciplineOfTheStudyGroup = IdDisciplineOfTheStudyGroup;
                disciplineOfTheStudyGroup.DisciplineId = DisciplineId;
                disciplineOfTheStudyGroup.StudyGroupId = StudyGroupId;

                
                try
                {
                    var validationResult = await disciplineOfTheStudyGroupValidator.ValidateAsync(disciplineOfTheStudyGroup);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateDisciplineStudyGroup", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateDisciplineStudyGroup", "AdminCRUD");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(disciplineOfTheStudyGroup), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PutAsync(apiUrl + "DisciplineOfTheStudyGroups/" + IdDisciplineOfTheStudyGroup, content);

                    return RedirectToAction("DisciplineStudyGroup", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на удаление данных о дисциплине учебной группы
        /// </summary>
        /// <param name="IdDisciplineOfTheStudyGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteStudyGroupDiscipline(int IdDisciplineOfTheStudyGroup)
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

                    using (var response = await httpClient.DeleteAsync(apiUrl + "DisciplineOfTheStudyGroups/" + IdDisciplineOfTheStudyGroup))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("DisciplineStudyGroup", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы дисциплины преподавателей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DisciplineTeacher()
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

                List<Discipline> disciplines = new List<Discipline>();
                List<Employee> employees = new List<Employee>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<Course> courses = new List<Course>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await client.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                }
                DisciplineEmployeeView disciplineEmployeeView = new DisciplineEmployeeView(disciplines, employees, disciplineEmployees, courses);
                return View(disciplineEmployeeView);
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка " + e.Message);
            }

        }


        /// <summary>
        /// POST запрос добавления дисциплины преподавателя
        /// </summary>
        /// <param name="DisciplineId"></param>
        /// <param name="EmployeeId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DisciplineTeacherNew(int DisciplineId, int EmployeeId)
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
                DisciplineEmployee disciplineEmployee = new DisciplineEmployee();
                disciplineEmployee.DisciplineId = DisciplineId;
                disciplineEmployee.EmployeeId = EmployeeId;

                
                try
                {
                    var validationResult = await disciplineEmployeeValidator.ValidateAsync(disciplineEmployee);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("DisciplineTeacher", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("DisciplineTeacher", "AdminCRUD");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(disciplineEmployee), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PostAsync(apiUrl + "DisciplineEmployee", content);

                    return RedirectToAction("DisciplineTeacher", "AdminCRUD");

                }

            }
            catch (Exception e)
            {
                return BadRequest("Ошибка добавления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных о дисциплине преподавателя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateDisciplineTeacher(int id)
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
                List<Discipline> disciplines = new List<Discipline>();
                List<Employee> employees = new List<Employee>();
                DisciplineEmployee disciplineEmployees = new DisciplineEmployee();
                List<Course> courses = new List<Course>();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await client.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Employees/All"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "DisciplineEmployee/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<DisciplineEmployee>(apiResponse);
                    }
                    using (var response = await client.GetAsync(apiUrl + "Courses"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        courses = JsonConvert.DeserializeObject<List<Course>>(apiResponse);
                    }
                }
                DisciplineEmployeeView disciplineEmployeeView = new DisciplineEmployeeView(disciplines, employees, new List<DisciplineEmployee> { disciplineEmployees }, courses);

                return View(disciplineEmployeeView);
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на изменение данных о дисциплины учебной группы
        /// </summary>
        /// <param name="IdDisciplineOfTheStudyGroup"></param>
        /// <param name="StudyGroupId"></param>
        /// <param name="DisciplineId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateDisciplineTeacher(int IdDisciplineEmployee, int DisciplineId, int EmployeeId)
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

                DisciplineEmployee disciplineEmployee = new DisciplineEmployee();
                disciplineEmployee.IdDisciplineEmployee = IdDisciplineEmployee;
                disciplineEmployee.DisciplineId = DisciplineId;
                disciplineEmployee.EmployeeId = EmployeeId;

                
                try
                {
                    var validationResult = await disciplineEmployeeValidator.ValidateAsync(disciplineEmployee);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("UpdateDisciplineTeacher", "AdminCRUD");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("UpdateDisciplineTeacher", "AdminCRUD");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(disciplineEmployee), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PutAsync(apiUrl + "DisciplineEmployee/" + IdDisciplineEmployee, content);

                    return RedirectToAction("DisciplineTeacher", "AdminCRUD");

                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных " + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос на удаление данных о дисциплине учебной группы
        /// </summary>
        /// <param name="IdDisciplineOfTheStudyGroup"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteDisciplineTeacher(int IdDisciplineEmployee)
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

                    using (var response = await httpClient.DeleteAsync(apiUrl + "DisciplineEmployee/" + IdDisciplineEmployee))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("DisciplineTeacher", "AdminCRUD");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления данных " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка представления страницы архива студента
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ArchiveStudent()
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

                    using (var response = await httpClient.GetAsync(apiUrl + "Students/Archive"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        students = JsonConvert.DeserializeObject<List<Student>>(apiResponse);
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
        /// Загрузка представления страницы архива преподавателя
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Archive()
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

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/Archive"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employees = JsonConvert.DeserializeObject<List<Employee>>(apiResponse);
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
       /// Метод восстановления преподавателя
       /// </summary>
       /// <param name="id"></param>
       /// <returns></returns>
        public async Task<IActionResult> RecoverTeacher(int id)
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

                    var response = await httpClient.GetAsync(apiUrl + "Employees/" + id);
                    response.EnsureSuccessStatusCode();

                    var employeeData = await response.Content.ReadAsStringAsync();

                    var employee = JsonConvert.DeserializeObject<Employee>(employeeData);
                    employee.IsDeleted = 0;

                    string json = JsonConvert.SerializeObject(employee);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Employees/" + id, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Archive", "AdminCRUD");
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
      /// Метод восстановления студента
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
        public async Task<IActionResult> RecoverStudent(int id)
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

                    var response = await httpClient.GetAsync(apiUrl + "Students/" + id);
                    response.EnsureSuccessStatusCode();

                    var studentData = await response.Content.ReadAsStringAsync();

                    var student = JsonConvert.DeserializeObject<Student>(studentData);
                    student.IsDeleted = 0;

                    string json = JsonConvert.SerializeObject(student);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Students/" + id, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ArchiveStudent", "AdminCRUD");
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
    }
}
