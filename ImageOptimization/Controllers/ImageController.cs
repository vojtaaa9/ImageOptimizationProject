using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
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
        private readonly IStoreAppContext _db = new ImageContext();
        private readonly int[] _sizes = { 2048, 1900, 1750, 1600, 1400, 1200, 900, 600, 300 };

        public ImageController() {}

        public ImageController(IStoreAppContext context)
        {
            _db = context;
        }

        // GET: Image
        public ActionResult Index(int count = 10, int page = 0)
        {
            // If page is lower than 1, reset
            if (page < 0)
                page = -1;

            int total = _db.SourceImages.Count();

            // If page is higher than total pages count
            if (page > total / count)
                page = -1;

            if (page == -1)
            {
                // return empty list which shows warning
                return View(new ListSourceImageViewModel { Page = 0, ImageItems = new List<ThumbImage>() });
            }

            // Load 30 SourceImages according to current page
            List<SourceImage> sourceImages = _db.SourceImages
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
                _db.SaveChanges();

                // Add it to colletion of thumbnails
                thumbnails.Add(thumbnail);
            }

            // Set Data to viewmodel
            var vm = new ListSourceImageViewModel
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
            SourceImage sourceImage = _db.SourceImages.Find(id);
            if (sourceImage == null)
            {
                return HttpNotFound();
            }

            // Update Data on Detail view, which cant be inserted into db at seed time
            Image vipsImage = Image.NewFromFile(sourceImage.AbsolutePath);

            sourceImage.Width = vipsImage.Width;
            sourceImage.Height = vipsImage.Height;
            vipsImage.Dispose();

            List<ThumbImage> thumbs = new List<ThumbImage>();

            // Generate set of 8 thumbnails
            foreach (var size in _sizes)
            {
                ThumbImage thumbnail = sourceImage.GetImage(Format.JPEG, size, sourceImage.Thumbnails);
                //ThumbImage thumbnailx = sourceImage.GetImage(Format.WebP, size, sourceImage.Thumbnails);

                SaveThumb(sourceImage, thumbnail, sourceImage.Thumbnails);
                //SaveThumb(sourceImage, thumbnailx, sourceImage.Thumbnails);

                thumbs.Add(thumbnail);
            }

                
            Format[] formats = { Format.GIF, Format.JPEG, Format.PNG, Format.WebP };

            // Generate format images
            List<Format[]> formatTransform = new List<Format[]> {
                new[] { Format.GIF, Format.JPEG },
                new[] { Format.GIF, Format.WebP },
                new[] { Format.PNG, Format.JPEG },
                new[] { Format.PNG, Format.WebP },
                new[] { Format.JPEG, Format.WebP },
                new[] { Format.WebP, Format.JPEG }
            };

            List<CompareImage> formatThumb = new List<CompareImage>();
            List<CompareImage> formatChangeThumb = new List<CompareImage>();

            foreach (Format[] format in formatTransform)
            {
                ThumbImage formatImage = sourceImage.GetImage(format[0], sourceImage.Width, sourceImage.Formats, sourceImage.Height);
                ThumbImage formatImagex = sourceImage.GetImage(format[1], sourceImage.Width, sourceImage.Formats, sourceImage.Height);

                if (formatImage.Format == Format.SVG)
                    continue;

                // Check if compare Image already exists
                CompareImage compare = null;
                if (_db.CompareImages.Any())
                {
                    compare = _db.CompareImages
                        .FirstOrDefault(i => i.Image1.AbsolutePath == formatImage.AbsolutePath 
                                             && i.Image2.AbsolutePath == formatImagex.AbsolutePath
                                             );
                }

                // If not, create one
                if (compare == null)
                {
                    compare = new CompareImage
                    {
                        Image1 = formatImage,
                        Image2 = formatImagex,
                        SSIM = ImageService.GetSSIM(formatImage.AbsolutePath, formatImagex.AbsolutePath)
                    };

                    _db.CompareImages.Add(compare);
                    _db.SaveChanges();
                }

                SaveThumb(sourceImage, formatImage, sourceImage.Formats);
                SaveThumb(sourceImage, formatImagex, sourceImage.Formats);

                formatThumb.Add(compare);
            }

            // Generate compression images
            foreach (Format format in formats)
            {
                ThumbImage compressImage = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Compression, sourceImage.Height);
                ThumbImage compressImagex = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Compression, sourceImage.Height, 75);

                if (compressImage.Format == Format.SVG)
                    continue;

                // Check if compare Image already exists
                CompareImage compare = null;
                if (_db.CompareImages.Any())
                {
                    compare = _db.CompareImages
                        .FirstOrDefault(i => i.Image1.AbsolutePath == compressImage.AbsolutePath && i.Image2.AbsolutePath == compressImagex.AbsolutePath);
                }

                // If not, create one
                if (compare == null)
                {
                    compare = new CompareImage
                    {
                        Image1 = compressImage,
                        Image2 = compressImagex,
                        SSIM = ImageService.GetSSIM(compressImage.AbsolutePath, compressImagex.AbsolutePath)
                    };

                    _db.CompareImages.Add(compare);
                    _db.SaveChanges();
                }

                SaveThumb(sourceImage, compressImage, sourceImage.Compression);
                SaveThumb(sourceImage, compressImagex, sourceImage.Compression);

                formatChangeThumb.Add(compare);
            }

            // Generate stripped images (remove metadata)
            foreach (Format format in formats)
            {
                ThumbImage strippedImage = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Metadata, sourceImage.Height);
                ThumbImage strippedImagex = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Metadata, sourceImage.Height, strip: true);

                SaveThumb(sourceImage, strippedImage, sourceImage.Metadata);
                SaveThumb(sourceImage, strippedImagex, sourceImage.Metadata);
            }

            // Save Changes if any
            if (_db.IsModified(sourceImage))
                _db.SaveChanges();


            // Create ViewModel
            SourceImageViewModel sourceImageViewModel = ImageService.GetSourceImageViewModel(sourceImage, thumbs);
            sourceImageViewModel.Formats = formatThumb;
            sourceImageViewModel.Compression = formatChangeThumb;

            return View("Details", sourceImageViewModel);
        }

        // GET: Image/FormatTest/5?format=1?strip=true&quality=90
        public ActionResult FormatTest(int? id, Format format, bool strip = false, int quality = 100)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SourceImage sourceImage = _db.SourceImages.Find(id);
            if (sourceImage == null)
            {
                return HttpNotFound();
            }

            // Convert the Image
            ThumbImage image = sourceImage.GetImage(format, sourceImage.Width, sourceImage.Thumbnails, q: quality, strip: strip);
            SaveThumb(sourceImage, image, sourceImage.Thumbnails);
            _db.SaveChanges();

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
                _db.SourceImages.Add(sourceImage);
                _db.SaveChanges();
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
            SourceImage sourceImage = _db.SourceImages.Find(id);
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
                _db.IsModified(sourceImage);
                _db.SaveChanges();
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
            SourceImage sourceImage = _db.SourceImages.Find(id);
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
            SourceImage sourceImage = _db.SourceImages.Find(id);
            _db.SourceImages.Remove(sourceImage ?? throw new InvalidOperationException());
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Image/TestVips
        public ActionResult TestVips()
        {
            string result = ModuleInitializer.VipsInitialized
            ? $"Inited libvips {Base.Version(0)}.{Base.Version(1)}.{Base.Version(2)}"
            : "Unable to init libvips";
            Debug.WriteLine(result);

            return View("TestVips", (object)result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SaveThumb(SourceImage sourceImage, ThumbImage thumbnail, List<ThumbImage> collection)
        {
            if (!_db.ThumbImages.AsEnumerable().Contains(thumbnail))
            {
                // Save new thumbnail to db
                _db.ThumbImages.Add(thumbnail);

                _db.MarkAsModified(sourceImage);
            }

            if (!collection.AsEnumerable().Contains(thumbnail))
            {
                collection.Add(thumbnail);
                _db.MarkAsModified(sourceImage);
            }
        }
    }
}
