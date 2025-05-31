using Boardium.HelperFuncs;
using Boardium.Models;

namespace BasicUnitTests {
    public class HelperFunctions {
        BorrowInfo[] _borrowInfo;
        [SetUp]
        public void Setup() {
            BorrowInfo firstBI = new BorrowInfo();
            firstBI.BorrowDate = DateTime.Now.AddDays(-2);
            firstBI.DueDate = DateTime.Now.AddDays(5);
            BorrowInfo secondBI = new BorrowInfo();
            BorrowInfo thirdBI = new BorrowInfo();
            _borrowInfo[0] = firstBI;
            _borrowInfo[1] = secondBI;
            _borrowInfo[2] = thirdBI;
        }

        [Test]
        public void DateIsBetweenDates() {
            Assert.Pass();
        }
    }
}
