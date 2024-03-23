using Microsoft.AspNetCore.Mvc;

namespace Mastery_Quotient.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
