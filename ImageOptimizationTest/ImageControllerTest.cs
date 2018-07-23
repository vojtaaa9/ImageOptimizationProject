using System.Web.Mvc;
using ImageOptimization.Controllers;
using NUnit.Framework;

namespace ImageOptimizationTest
{
    [TestFixture]
    public class ImageControllerTest
    {
        [Test]
        public void ShouldOpenHome()
        {
            ImageController controller = new ImageController();

            ViewResult result = controller.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Model);
        }

        [Test]
        public void ShouldRunNetVips()
        {
            ImageController controller = new ImageController();

            var result = controller.TestVips() as ViewResult;

            Assert.NotNull(result);
            Assert.IsTrue(result.Model.ToString().Contains("Inited libvips"));
        }
    }
}
