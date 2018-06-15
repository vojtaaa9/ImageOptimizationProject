# Image Optimization Project

This repository contains all sources to sample web application, where different optimization techniques will be tested. It is made simple for testing images in the web, there is no specific coding style, no tests, yet if you are familiar with ASP.NET MVC Development, you should feel home.

**Requirements:**

* .NET Framework 4.6
* Visual Studio 2017
* libvips v8.6.3
* [npm](https://www.npmjs.com/get-npm) and [svgo](https://github.com/svg/svgo) for minifying SVG's
* [ImageMagick](http://www.imagemagick.org/script/index.php) for ssim Index and testing


## Image Dataset

Images should be stored in `ImageOptimization/images` :(. Unfortunately, I can't give you the images, since I do not own them. But you can go ahead do the tests with some public image datasets. Please be aware, that lots of the images online are already somehow compressed or optimized, which makes it difficult to show real impact. More about it on [kornelski website](https://kornel.ski/en/faircomparison).


## How to get up and running: 

1. Download vips and install it.
1. Get raster image dataset and save it in `ImageOptimization/images`. **I strongly recommend normalizing the dataset** with `tiff-normalize.ps1` script. It will resize all images to max width of 2048 px, which is excellent for further testing.
1. Get vector image dataset and use the Script `svgo-optimize.ps1` to generate minified SVG's. Then copy the folder (with SVG's and folder _out_ with minified SVG's)to `ImageOptimization/images`. Now you should have
1. Build the Project
1. `InitialCreate` the Database using Entity Framework 6 (this will also seed the database from images folder)
1. Run it
1. You should see nice message from the application, displaying the images (if you don't, look at troubleshooting section)


### Installing vips

For Windows:

Download `vips-dev-w64-web-x.y.z.zip` from the download area of libvips. The last one is `vips-dev-w64-web-8.6.3-1.zip` at the time of writing.
Unzip `vips-dev-w64-web-x.y.z.zip` to a suitable location that can easily be remembered. For example: `C:\vips-dev-8.6.3`.
Add the bin (e.g. `C:\vips-dev-8.6.3\bin`) directory to your **PATH** environment variable:

In Search, search for **SystemPropertiesAdvanced** and then select it.
Click **Environment Variables**. In the section **System Variables**, find the **PATH** environment variable and select it. Click Edit.
In the Edit System Variable window, click the New button, which will add a line at the end of the list. Add the libvips bin directory (`C:\vips-dev-8.6.3\bin` in our example) and hit Enter. Click OK. Close all remaining windows by clicking OK.

Now libvips would have been installed successfully, **reopen Visual Studio** or the **Command prompt window** (to ensure that the environment is updated), and run your code that has references to NetVips.

To make sure, it worked you can run: `vips.exe -v`

If you see something like this: `vips-8.6.3-Thu Mar  8 15:18:35 UTC 2018`, it works.

## Testing Images
All thumbnail images will be saved in `ImageOptimization/thumbnails` folder. The names are generated automatically and will be in this format:
`"th_{w}x{h}_{q}_{src_name}{opt}.{format}"`. `w` refers to width of the thumbnail, `h` to height, `q` to Quality settings, `src_name` to the name of the source image, `opt` is optinal number, in case the file already exists on the file system and lastly `format` is the format ending.

For example: source image with name **lizard.tif** will have this thumbnail generated and saved: `th_200x130_100_lizard.jpeg`

```
> magick compare -verbose -metric SSIM birthday.png birthday-768.png NULL:
birthday.png PNG 1024x768 1024x768+0+0 8-bit sRGB 346856B 0.016u 0:00.021
birthday-768.png PNG 768x576 768x576+0+0 8-bit sRGB 307858B 0.016u 0:00.016
Image: birthday.png
Offset: 0,0
  Channel distortion: SSIM
    red: 0.509967
    green: 0.446983
    blue: 0.375166
    all: 0.444038
```

## Troubleshooting

**libvips:**
Make sure `libvips` is installed correctly. If you have problems regarding assemblies and can't even start up the application, make sure, that you are building against `x64`. In Solution Explorer right-click Project > Properties > Select Web and at Server Section set Bitness to x64. This is because libvips can't run on 32 bit server.

**Database corupt?** Just run `Update-Database -TargetMigration:0 -force` from Package Manager Console. This will wipe all data and you will have fresh start. With `Update-Database` Entity Framework will run all migrations and apply them to the database.