using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers;

public class ReliefTeamController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}