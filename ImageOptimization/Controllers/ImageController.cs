using System.Web.Mvc;

namespace ImageOptimization.Controllers
{
    public class ImageController : Controller
    {
        // GET: Image
        public ActionResult Index()
        {
            return View();
        }
    }
}