using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers;

public class FinancialAllocationController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}