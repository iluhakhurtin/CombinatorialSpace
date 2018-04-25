using CombinatorialSpace.BinaryVectors;
using CombinatorialSpace_Tests.BaseClasses;
using System;
using Xunit;
using System.Linq;

namespace CombinatorialSpace_Tests
{
    public class CombinatorialSpaceBuilder_Test : BaseTest
    {
        ICombinatorialSpaceBuilder combinatorialSpaceBuilder;

        public CombinatorialSpaceBuilder_Test()
        {
            this.combinatorialSpaceBuilder = new CombinatorialSpaceBuilder();
        }

        [Fact]
        public void Can_build_combinatorial_space()
        {
            //see page 7 in https://habrahabr.ru/post/326334/
            int combinatorialSpaceLength = 60000;
            int numberOfTrackingBits = 32;
            int trackingBinaryVectorLength = 256;

            var combinatorialSpace = this.combinatorialSpaceBuilder.Build(combinatorialSpaceLength, numberOfTrackingBits, trackingBinaryVectorLength);
            var combinatorialSpaceList = combinatorialSpace.ToList();

            int actualCount = 0;
            foreach (var point in combinatorialSpaceList)
            {
                actualCount++;
                int sameItemsCount = combinatorialSpaceList.Where(p => point == p).Count();

                Assert.Equal(1, sameItemsCount);
            }

            Assert.Equal(combinatorialSpaceLength, actualCount);
        }
    }
}
