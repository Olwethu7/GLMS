using Xunit;

namespace GLMS.Tests.Models
{
    public class SimpleModelTests
    {
        [Fact]
        public void Test_Ten_Is_Greater_Than_Five()
        {
            Assert.True(10 > 5);
        }

        [Fact]
        public void Test_String_Equals_Expected()
        {
            string expected = "GLMS";
            string actual = "GLMS";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_List_Is_Not_Null()
        {
            var list = new List<string>();
            Assert.NotNull(list);
        }
    }
}