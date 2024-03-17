using Firebase.Storage;
using Mastery_Quotient.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Mastery_Quotient.Controllers
{
    public class TeacherController : Controller
    {

        private readonly IConfiguration configuration;

        private static string Bucket = "mastquo.appspot.com";

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
        public async Task<IActionResult> TeacherWindowMaterial(string nameMaterial, int typeMaterial, int disciplineMaterial, IFormFile file )
        {
            try
            {
                if(file == null || file.Length == 0)
                {
                    return BadRequest("Файл не был загружен");
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fileUrl = await Upload(file.OpenReadStream(), fileName);


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


                StringContent content = new StringContent(JsonConvert.SerializeObject(material), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var responseStudent = await httpClient.PostAsync(apiUrl + "Materials", content);

                    if (responseStudent.IsSuccessStatusCode)
                    {
                        return RedirectToAction("MainTeacher", "Teacher");
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

        

        public async Task<string> Upload(Stream stream, string fileName)
        {
            var cancellation = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(Bucket);
            string path = "material/" + fileName;
            var uploadTask = firebaseStorage.Child(path).PutAsync(stream, cancellation.Token);
            var fileUrl = await uploadTask;

            return fileUrl;
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
                return BadRequest();
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
                return BadRequest();
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

                    employee.PhotoEmployee = await UploadPhoto(photo.OpenReadStream(), fileName);

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

        public async Task<string> UploadPhoto(Stream stream, string fileName)
        {
            var cancellation = new CancellationTokenSource();

            var firebaseStorage = new FirebaseStorage(Bucket);
            string path = "photoProfile / " + fileName;
            var uploadTask = firebaseStorage.Child(path).PutAsync(stream, cancellation.Token);
            var fileUrl = await uploadTask;

            return fileUrl;
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

                    using (var response = await httpClient.GetAsync(apiUrl + "Materials"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        materials = JsonConvert.DeserializeObject<List<Material>>(apiResponse);
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

        public IActionResult FileMaterial(string nameFile)
        {

            ViewBag.NameFile = nameFile;
            return View();

        }
    }
}
