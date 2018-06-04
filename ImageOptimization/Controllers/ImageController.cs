using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ImageOptimization.DataPersistenceLayer;
using ImageOptimization.Models;
using ImageOptimization.Services;
using ImageOptimization.ViewModels;
using NetVips;

namespace ImageOptimization.Controllers
{
    public class ImageController : Controller
    {
        private ImageContext db = new ImageContext();
        private readonly ImageService imageService = new ImageService();

        // GET: Image
        public ActionResult Index(int count = 30, int page = 0)
        {
            // If page is lower than 1, reset
            if (page < 0)
                page = -1;

            int total = db.SourceImages.Count();

            // If page is higher than total pages count
            if (page > (total - count) / count)
                page = -1;

            if (page == -1)
            {
                // return empty list which shows warning
                return View(new ListSourceImageViewModel() { Page = 0, ImageItems = new List<SourceImage>() });
            }

            List<SourceImage> pageItems = db.SourceImages
                .OrderBy(i => i.FileName)
                .Skip(page * count)
                .Take(count)
                .ToList();

            var vm = new ListSourceImageViewModel()
            {
                Page = page,
                ImageItems = pageItems
            };

            return View(vm);
        }

        // GET: Image/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SourceImage sourceImage = db.SourceImages.Find(id);
            if (sourceImage == null)
            {
                return HttpNotFound();
            }

            var sourceImageViewModel = ImageService.GetSourceImageViewModel(sourceImage);

            return View("Details", sourceImageViewModel);
        }

        // GET: Image/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Image/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Path,AltText")] SourceImage sourceImage)
        {
            if (ModelState.IsValid)
            {
                db.SourceImages.Add(sourceImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sourceImage);
        }

        // GET: Image/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SourceImage sourceImage = db.SourceImages.Find(id);
            if (sourceImage == null)
            {
                return HttpNotFound();
            }
            return View(sourceImage);
        }

        // POST: Image/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Path,AltText")] SourceImage sourceImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sourceImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sourceImage);
        }

        // GET: Image/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SourceImage sourceImage = db.SourceImages.Find(id);
            if (sourceImage == null)
            {
                return HttpNotFound();
            }
            return View(sourceImage);
        }

        // POST: Image/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SourceImage sourceImage = db.SourceImages.Find(id);
            db.SourceImages.Remove(sourceImage);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Image/TestVips
        public ActionResult TestVips()
        {
            string result = ModuleInitializer.VipsInitialized
            ? $"Inited libvips {Base.Version(0)}.{Base.Version(1)}.{Base.Version(2)}"
            : "Unable to init libvips";
            System.Diagnostics.Debug.WriteLine(result);

            return View("TestVips", (object)result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
