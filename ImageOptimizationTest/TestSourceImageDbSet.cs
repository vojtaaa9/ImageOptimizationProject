using System.Linq;
using ImageOptimization.Models;

namespace ImageOptimizationTest
{
    class TestSourceImageDbSet : TestDbSet<SourceImage>
    {
        public override SourceImage Find(params object[] keyValues)
        {
            return this.SingleOrDefault(product => product.ID == (int)keyValues.Single());
        }
    }
}
