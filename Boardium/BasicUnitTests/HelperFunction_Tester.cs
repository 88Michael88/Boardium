using Boardium.HelperFuncs;
using Boardium.Models;

namespace BasicUnitTests {
    public class HelperFunctions {
        BorrowInfo[] _borrowInfo = new BorrowInfo[3];
        [SetUp]
        public void Setup() {
            BorrowInfo firstBI = new BorrowInfo();
            firstBI.BorrowDate = DateTime.Now.AddDays(-2);
            firstBI.DueDate = DateTime.Now.AddDays(5);
            BorrowInfo secondBI = new BorrowInfo();
            secondBI.BorrowDate = DateTime.Now.AddDays(7);
            secondBI.DueDate = DateTime.Now.AddDays(12);
            BorrowInfo thirdBI = new BorrowInfo();
            thirdBI.BorrowDate = DateTime.Now.AddDays(13);
            thirdBI.DueDate = DateTime.Now.AddDays(18);
            _borrowInfo[0] = firstBI;
            _borrowInfo[1] = secondBI;
            _borrowInfo[2] = thirdBI;
        }

        [Test]
        public void DateIsBetweenDates() {

        }
    }
}
