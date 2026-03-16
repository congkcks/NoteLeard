using Microsoft.AspNetCore.Mvc;

namespace NoteLearn.Controllers;

public class ProgressController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
