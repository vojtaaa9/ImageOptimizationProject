# Image Optimization Project

This repository contains all sources to sample web application, where different optimization techniques will be tested. It is made simple for testing images in the web, there is no specific coding style, no tests, yet if you are familiar with ASP.NET MVC Development, you should feel home.


## Image Dataset

Images should be stored in `ImageOptimization/images` :(. Unfortunately, I can't give you the images, since I do not own them. But you can go ahead do the tests with some public image datasets.


## How to get up and running: 

1. Download vips and install it.
2. Get Image dataset and save it in `ImageOptimization/images`
3. Build the Project
4. `InitialCreate` the Database using Entity Framework 6 (this will also seed the database from images folder)
5. Run it
6. You should see


### Installing vips

For Windows:

Download `vips-dev-w64-web-x.y.z.zip` from the download area of libvips. The last one is `vips-dev-w64-web-8.6.3-1.zip` at the time of writing.
Unzip `vips-dev-w64-web-x.y.z.zip` to a suitable location that can easily be remembered. For example: `C:\vips-dev-8.6.3`.
Add the bin (e.g. `C:\vips-dev-8.6.3\bin`) directory to your **PATH** environment variable:

In Search, search for **SystemPropertiesAdvanced** and then select it.
Click **Environment Variables**. In the section **System Variables**, find the **PATH** environment variable and select it. Click Edit.
In the Edit System Variable window, click the New button, which will add a line at the end of the list. Add the libvips bin directory (`C:\vips-dev-8.6.3\bin` in our example) and hit Enter. Click OK. Close all remaining windows by clicking OK.

Now libvips would have been installed successfully, **reopen Visual Studio** or the **Command prompt window** (to ensure that the environment is updated), and run your code that has references to NetVips.

To make sure, it worked you can run: ``` vips.exe -v ```

If you see something like this: ```vips-8.6.3-Thu Mar  8 15:18:35 UTC 2018 ```, it works.

## Troubleshooting

Make sure `libvips` ist installed correctly. If you have problems regarding assemblies and can't even start up the application, make sure, that you are building against `x64`. In Solution Explorer right-click Project > Properties > Select Web and at Server Section set Bitness to x64. This is because libvips can't run on 32 bit server.