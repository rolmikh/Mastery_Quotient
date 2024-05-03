using Firebase.Storage;
using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Mastery_Quotient.Service;
using static System.Net.Mime.MediaTypeNames;
using FluentValidation;
using Mastery_Quotient.ModelsValidation;

namespace Mastery_Quotient.Controllers
{
    public class TeacherController : Controller
    {

        private readonly IConfiguration configuration;

        FirebaseService firebaseService = new FirebaseService();

        private readonly IValidator<Material> materialValidator;

        private readonly IValidator<Employee> employeeValidator;

        TokenService tokenService = new TokenService();

        public TeacherController(IConfiguration configuration, IValidator<Material> materialValidator, IValidator<Employee> employeeValidator, ILogger<AdminController> logger)
        {
            this.configuration = configuration;
            this.materialValidator = materialValidator;
            this.employeeValidator = employeeValidator;
            _logger = logger;
        }

        private readonly ILogger<AdminController> _logger;
        public IActionResult MainTeacher()
        {
            return View();
        }

        public IActionResult TeacherWindowTest() 
        {
            return View();
        }

        /// <summary>
        /// Загрузка представления страницы добавления материалов
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public async Task<IActionResult> TeacherWindowMaterial()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();

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

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                    }
                }
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                MaterialTeacher material = new MaterialTeacher(employee, disciplines, discipline, typeMaterials);
                return View(material);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
        /// <summary>
        /// POST запрос добавления материалов
        /// </summary>
        /// <param name="nameMaterial"></param>
        /// <param name="typeMaterial"></param>
        /// <param name="disciplineMaterial"></param>
        /// <param name="file"></param>
        /// <param name="filePhoto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TeacherWindowMaterial(string nameMaterial, string videoMaterial, int typeMaterial, int disciplineMaterial, IFormFile file, IFormFile filePhoto)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                Material material = new Material();

                if (videoMaterial == null || videoMaterial.Length == 0)
                {
                    if (file == null || file.Length == 0)
                    {
                        return BadRequest("Файл не был загружен");
                    }
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string fileUrl = await firebaseService.UploadMaterial(file.OpenReadStream(), fileName);
                    material.FileMaterial = fileUrl;

                }
                else
                {
                    material.FileMaterial = videoMaterial;

                }


                if (filePhoto == null || filePhoto.Length == 0)
                {
                    return BadRequest("Файл не был загружен");
                }


                

                string fileNamePhoto = Guid.NewGuid().ToString() + Path.GetExtension(filePhoto.FileName);
                string fileUrlPhoto = await firebaseService.UploadPhotoMaterial(filePhoto.OpenReadStream(), fileNamePhoto);



                var apiUrl = configuration["AppSettings:ApiUrl"];

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");


                material.NameMaterial = nameMaterial;
                material.DateCreatedMaterial = DateTime.Now;
                material.DisciplineId = disciplineMaterial;
                material.TypeMaterialId = typeMaterial;
                material.EmployeeId = id;
                material.PhotoMaterial = fileUrlPhoto;
                material.IsDeleted = 0;

                
                try
                {
                    var validationResult = await materialValidator.ValidateAsync(material);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("TeacherWindowMaterial", "Teacher");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TeacherWindowMaterial", "Teacher");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(material), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PostAsync(apiUrl + "Materials", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("MaterialsTeacher", "Teacher");
                    }
                    else
                    {
                        return RedirectToAction("TeacherWindowMaterial", "Teacher");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// Загрузка представления страницы личного кабинета преподавателя
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PersonalAccountTeacher()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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
                return BadRequest("Ошибка авторизации!");
            }
        }

        /// <summary>
        /// POST запрос изменения данных в личном кабинете
        /// </summary>
        /// <param name="surnameUser"></param>
        /// <param name="nameUser"></param>
        /// <param name="middleNameUser"></param>
        /// <param name="emailUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PersonalAccountTeacher(string surnameUser, string nameUser, string middleNameUser, string emailUser)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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
                            return RedirectToAction("PersonalAccountTeacher", "Teacher");
                        }

                    }
                    catch (Exception ex)
                    {
                        var errorMessages = new List<string> { ex.Message };
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("PersonalAccountTeacher", "Teacher");
                    }
                    string json = JsonConvert.SerializeObject(employee);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Employees/" + id, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("PersonalAccountTeacher", "Teacher");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка");
            }
        }


        /// <summary>
        /// POST запрос обновления фотографии в личном кабинете
        /// </summary>
        /// <param name="photo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdatePhoto(IFormFile photo)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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

                    var response = await httpClient.GetAsync(apiUrl + "Employees/" + id);
                    response.EnsureSuccessStatusCode();
                    var employeeData = await response.Content.ReadAsStringAsync();

                    var employee = JsonConvert.DeserializeObject<Employee>(employeeData);

                    employee.PhotoEmployee = await firebaseService.UploadPhoto(photo.OpenReadStream(), fileName);

                    string json = JsonConvert.SerializeObject(employee);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Employees/" + id, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("PersonalAccountTeacher", "Teacher");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        
        /// <summary>
        /// Метод выхода из аккаунта
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
        /// Загрузка представления страницы материалов преподавателя
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MaterialsTeacher()
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
                    if(refreshToken != null)
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
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();
                List<Material> materials = new List<Material>();

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

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
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
                        using (var response = await httpClient.GetAsync(apiUrl + "Materials/All"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
                        }
                    }
                   
                }
                List<Material> materialsList = materials.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                MaterialsViewTeacher material = new MaterialsViewTeacher(employee, disciplines, discipline, typeMaterials, materialsList);
                return View(material);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }


        }

        /// <summary>
        /// POST запрос поиска материалов преподавателя
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MaterialsTeacher(string Search)
        {
            try
            {
                if (Search != null)
                {
                    var token = TokenService.token;

                    if (tokenService.IsTokenExpired(token))
                    {
                        var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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
                            return RedirectToAction("MaterialsTeacher", "Teacher");

                        }

                        return RedirectToAction("MaterialsTeacher", "Teacher");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("MaterialsTeacher", "Teacher");
                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }



        }

        /// <summary>
        /// POST запрос фильтрации материалов преподавателя
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
                        var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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
                            return RedirectToAction("MaterialsTeacher", "Teacher");

                        }

                        return RedirectToAction("MaterialsTeacher", "Teacher");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("MaterialsTeacher", "Teacher");
                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }

       


        /// <summary>
        /// Загрузка представления страницы изменения материала
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateMaterial(int id)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                int idUser = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();
                Material materials = null;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + idUser))
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

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeMaterials = JsonConvert.DeserializeObject<List<TypeMaterial>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Materials/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<Material>(apiResponse);
                    }
                }

                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                MaterialsViewTeacher material = new MaterialsViewTeacher(employee, disciplines, discipline, typeMaterials,new List<Material> { materials });
                return View(material);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// POST запрос изменения данных материала
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateMaterial(int IdMaterial, string nameMaterial, int typeMaterial, int disciplineMaterial, IFormFile file, IFormFile filePhoto)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
                    if (refreshToken != null)
                    {
                        TokenService.token = refreshToken;
                    }
                    else
                    {
                        return RedirectToAction("Authorization", "Home");
                    }
                }
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Файл не был загружен");
                }
                if (filePhoto == null || filePhoto.Length == 0)
                {
                    return BadRequest("Файл не был загружен");
                }


                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fileUrl = await firebaseService.UploadMaterial(file.OpenReadStream(), fileName);

                string fileNamePhoto = Guid.NewGuid().ToString() + Path.GetExtension(filePhoto.FileName);
                string fileUrlPhoto = await firebaseService.UploadPhotoMaterial(filePhoto.OpenReadStream(), fileNamePhoto);



                var apiUrl = configuration["AppSettings:ApiUrl"];

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                Material material = new Material();
                material.IdMaterial = IdMaterial;
                material.NameMaterial = nameMaterial;
                material.DateCreatedMaterial = DateTime.Now;
                material.FileMaterial = fileUrl;
                material.DisciplineId = disciplineMaterial;
                material.TypeMaterialId = typeMaterial;
                material.EmployeeId = id;
                material.PhotoMaterial = fileUrlPhoto;
                material.IsDeleted = 0;

               
                try
                {
                    var validationResult = await materialValidator.ValidateAsync(material);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                        TempData["ErrorValidation"] = errorMessages;
                        return RedirectToAction("TeacherWindowMaterial", "Teacher");
                    }

                }
                catch (Exception ex)
                {
                    var errorMessages = new List<string> { ex.Message };
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TeacherWindowMaterial", "Teacher");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(material), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PutAsync(apiUrl + "Materials/" + IdMaterial, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("MaterialsTeacher", "Teacher");
                    }
                    else
                    {
                        return RedirectToAction("TeacherWindowMaterial", "Teacher");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных" + ex.Message);
            }
        }

        /// <summary>
        /// POST запрос удаления материала
        /// </summary>
        /// <param name="IdMaterial"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int IdMaterial)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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

                    using (var response = await httpClient.DeleteAsync(apiUrl + "Materials/" + IdMaterial))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("MaterialsTeacher", "Teacher");
            }
            catch
            {
                return BadRequest("Ошибка удаления данных!");
            }
        }

        /// <summary>
        /// POST запрос восстановления материала
        /// </summary>
        /// <param name="IdMaterial"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RecoverMaterial(int IdMaterial)
        {
            try
            {
                var token = TokenService.token;

                if (tokenService.IsTokenExpired(token))
                {
                    var refreshToken = await tokenService.RefreshToken(token, "Преподаватель");
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

                    var response = await httpClient.GetAsync(apiUrl + "Materials/" + IdMaterial);
                    response.EnsureSuccessStatusCode();
                    var materialData = await response.Content.ReadAsStringAsync();

                    var material = JsonConvert.DeserializeObject<Material>(materialData);

                    material.IsDeleted = 0;
                    string json = JsonConvert.SerializeObject(material);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Materials/" + IdMaterial, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("MaterialsTeacher", "Teacher");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
