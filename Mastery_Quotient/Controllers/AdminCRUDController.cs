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

        public AdminCRUDController(IConfiguration configuration, IValidator<TypeMaterial> typeMaterialValidator, IValidator<TestParameter> testParameterValidator)
        {
            this.configuration = configuration;
            this.typeMaterialValidator = typeMaterialValidator;
            this.testParameterValidator = testParameterValidator;
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

    }
}
