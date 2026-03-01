using DMP_06.Models;
using DMP_06.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DMP_06.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View(new PredictPageVm());
    }

    public IActionResult Privacy()
    {
        return View();
    }

}
