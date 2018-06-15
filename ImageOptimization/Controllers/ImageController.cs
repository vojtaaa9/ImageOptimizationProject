﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ImageOptimization.DataPersistenceLayer;
using ImageOptimization.Enums;
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
        private readonly int[] sizes = { 300, 600, 900, 1200, 1400, 1600, 1750, 1900 };

        // GET: Image
        public ActionResult Index(int count = 10, int page = 0)
        {
            // If page is lower than 1, reset
            if (page < 0)
                page = -1;

            int total = db.SourceImages.Count();

            // If page is higher than total pages count
            if (page > total / count)
                page = -1;

            if (page == -1)
            {
                // return empty list which shows warning
                return View(new ListSourceImageViewModel() { Page = 0, ImageItems = new List<ThumbImage>() });
            }

            // Load 30 SourceImages according to current page
            List<SourceImage> sourceImages = db.SourceImages
                .Include(i => i.Thumbnails)
                .OrderBy(i => i.ID)
                .Skip(page * count)
                .Take(count)
                .ToList();

            // Save References to thumbnails
            List<ThumbImage> thumbnails = new List<ThumbImage>();

            foreach (var sourceImage in sourceImages)
            {
                ThumbImage thumbnail = sourceImage.GetImage(Format.JPEG, 200, sourceImage.Thumbnails, q: 75);

                SaveThumb(sourceImage, thumbnail, sourceImage.Thumbnails);
                db.SaveChanges();

                // Add it to colletion of thumbnails
                thumbnails.Add(thumbnail);
            }

            // Set Data to viewmodel
            var vm = new ListSourceImageViewModel()
            {
                Page = page,
                ImageItems = thumbnails
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

            // Update Data on Detail view, which cant be inserted into db at seed time
            Image VipsImage = Image.NewFromFile(sourceImage.AbsolutePath);

            sourceImage.Width = VipsImage.Width;
            sourceImage.Height = VipsImage.Height;

            VipsImage.Dispose();

            List<ThumbImage> thumbs = new List<ThumbImage>();

            // Generate set of 8 thumbnails
            foreach (int size in sizes)
            {
                ThumbImage thumbnail = sourceImage.GetImage(Format.JPEG, size, sourceImage.Thumbnails);

                SaveThumb(sourceImage, thumbnail, sourceImage.Thumbnails);

                thumbs.Add(thumbnail);
            }


            // Generate format images
            Format[] formats = new Format[] { Format.GIF, Format.JPEG, Format.PNG, Format.WebP};

            foreach(Format format in formats)
            {
                ThumbImage formatImage = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Formats);

                SaveThumb(sourceImage, formatImage, sourceImage.Formats);
            }


            // Generate compression images
            foreach (Format format in formats)
            {
                ThumbImage compressImage = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Compression, q: 75);

                SaveThumb(sourceImage, compressImage, sourceImage.Compression);
            }

            // Generate stripped images (remove metadata)
            foreach (Format format in formats)
            {
                ThumbImage strippedImage = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Metadata, strip: true);

                SaveThumb(sourceImage, strippedImage, sourceImage.Metadata);
            }

            // Save Changes if any
            if (db.Entry(sourceImage).State == EntityState.Modified)
                db.SaveChanges();


            // Create ViewModel
            SourceImageViewModel sourceImageViewModel = ImageService.GetSourceImageViewModel(sourceImage, thumbs);

            return View("Details", sourceImageViewModel);
        }

        // GET: Image/FormatTest/5?format=1?strip=true&quality=90
        public ActionResult FormatTest(int? id, Format format, bool strip = false, int quality = 100)
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

            // Convert the Image
            ThumbImage image = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Thumbnails, q: quality, strip: strip);
            SaveThumb(sourceImage, image, sourceImage.Thumbnails);
            db.SaveChanges();

            // Return the file back
            return File(image.RelativePath, "image/"+image.getFormat().ToLower());
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

        private void SaveThumb(SourceImage sourceImage, ThumbImage thumbnail, List<ThumbImage> collection)
        {
            if (!db.ThumbImages.AsEnumerable().Contains(thumbnail))
            {
                // Save new thumbnail to db
                db.ThumbImages.Add(thumbnail);

                db.Entry(sourceImage).State = EntityState.Modified;
            }

            if (!collection.AsEnumerable().Contains(thumbnail))
            {
                collection.Add(thumbnail);
                db.Entry(sourceImage).State = EntityState.Modified;
            }
        }
    }
}
