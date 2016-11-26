using Microsoft.AspNetCore.Mvc;
using OnlineServerWatch.Models.Connections;

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