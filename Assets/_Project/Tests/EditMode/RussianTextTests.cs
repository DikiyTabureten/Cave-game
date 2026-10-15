using NUnit.Framework;

namespace CaveGame.Tests
{
    public class RussianTextTests
    {
        [TestCase(1, "1 шаг")]
        [TestCase(3, "3 шага")]
        [TestCase(4, "4 шага")]
        [TestCase(5, "5 шагов")]
        [TestCase(11, "11 шагов")]
        [TestCase(12, "12 шагов")]
        [TestCase(21, "21 шаг")]
        [TestCase(22, "22 шага")]
        [TestCase(0, "0 шагов")]
        public void Steps_UsesCorrectForm(int count, string expected)
        {
            Assert.AreEqual(expected, RussianText.Steps(count));
        }
    }
}
