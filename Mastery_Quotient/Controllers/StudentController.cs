using Microsoft.AspNetCore.Mvc;

namespace Mastery_Quotient.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult MainStudent()
        {
            return View();
        }
    }
}
