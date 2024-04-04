using Firebase.Storage;
using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Mastery_Quotient.Service;
using static System.Net.Mime.MediaTypeNames;

namespace Mastery_Quotient.Controllers
{
    public class TeacherController : Controller
    {

        private readonly IConfiguration configuration;

        FirebaseService firebaseService = new FirebaseService();

        public TeacherController(IConfiguration configuration, ILogger<AdminController> logger)
        {
            this.configuration = configuration;
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

        [HttpGet]
        public async Task<IActionResult> TeacherWindowMaterial()
        {
            try
            {
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TypeMaterial> typeMaterials = new List<TypeMaterial>();

                using (var httpClient = new HttpClient())
                {
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

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials"))
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

        [HttpPost]
        public async Task<IActionResult> TeacherWindowMaterial(string nameMaterial, int typeMaterial, int disciplineMaterial, IFormFile file, IFormFile filePhoto)
        {
            try
            {
                if(file == null || file.Length == 0)
                {
                    return BadRequest("Файл не был загружен");
                }
                if(filePhoto == null || filePhoto.Length == 0)
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

                material.NameMaterial = nameMaterial;
                material.DateCreatedMaterial = DateTime.Now;
                material.FileMaterial = fileUrl;
                material.DisciplineId = disciplineMaterial;
                material.TypeMaterialId = typeMaterial;
                material.EmployeeId = id;
                material.PhotoMaterial = fileUrlPhoto;


                StringContent content = new StringContent(JsonConvert.SerializeObject(material), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
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

        

        

        [HttpGet]
        public async Task<IActionResult> PersonalAccountTeacher()
        {
            try
            {
                
                int id = int.Parse(TempData["AuthUser"].ToString());
                

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Role> roles = new List<Role>();

                using (var httpClient = new HttpClient())
                {
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


        [HttpPost]
        public async Task<IActionResult> PersonalAccountTeacher(string surnameUser, string nameUser, string middleNameUser, string emailUser, string passwordUser, string saltUser)
        {
            try
            {
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                employee.IdEmployee = id;
                employee.SurnameEmployee = surnameUser;
                employee.NameEmployee = nameUser;
                employee.MiddleNameEmployee = middleNameUser;
                employee.EmailEmployee = emailUser;
                employee.PasswordEmployee = passwordUser;
                employee.SaltEmployee = saltUser;
                employee.RoleId = 2;
                employee.IsDeleted = 0;


                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");

                    var response = await httpClient.PutAsync(apiUrl + "Employees/" + id, content);

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

        [HttpPost]
        public async Task<IActionResult> UpdatePhoto(IFormFile photo)
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

        [HttpGet]
        public async Task<IActionResult> MaterialsTeacher()
        {
            try
            {
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

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeMaterials"))
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
                        using (var response = await httpClient.GetAsync(apiUrl + "Materials"))
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

        [HttpPost]
        public async Task<IActionResult> MaterialsTeacher(string Search)
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
                        var response = await httpClient.GetAsync(apiUrl + "Materials/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);

                        TempData["Search"] = JsonConvert.SerializeObject(materials);

                        return RedirectToAction("MaterialsTeacher", "Teacher");
                    }
                }
                else
                {
                    return RedirectToAction("MaterialsTeacher", "Teacher");
                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }



        }


        [HttpPost]
        public async Task<IActionResult> FiltrationMaterial(int typeId, int disciplineID)
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

                        return RedirectToAction("MaterialsTeacher", "Teacher");

                    }
                }
                else
                {
                    return RedirectToAction("MaterialsTeacher", "Teacher");
                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }

        public IActionResult FileMaterial(string nameFile)
        {

            ViewBag.NameFile = nameFile;
            return View();

        }

        [HttpGet]
        public async Task<IActionResult> UpdateMaterial(int id)
        {
            try
            {

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

        [HttpPost]
        public async Task<IActionResult> UpdateMaterial()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int IdMaterial)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
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
    }
}
