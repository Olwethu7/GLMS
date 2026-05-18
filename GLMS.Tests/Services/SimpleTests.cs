using Xunit;

namespace GLMS.Tests.Services
{
    public class SimpleTests
    {
        [Fact]
        public void Test_One_Plus_One_Equals_Two()
        {
            Assert.Equal(2, 1 + 1);
        }

        [Fact]
        public void Test_True_Is_True()
        {
            Assert.True(true);
        }

        [Fact]
        public void Test_String_Is_Not_Empty()
        {
            string result = "Hello";
            Assert.False(string.IsNullOrEmpty(result));
        }
    }
}