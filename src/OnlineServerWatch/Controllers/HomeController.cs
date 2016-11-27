using Microsoft.AspNetCore.Mvc;

namespace OnlineServerWatch.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}