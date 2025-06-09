using Boardium.Models;

namespace Boardium.HelperFuncs {
    public class DateBetweenChecker {
        public bool DateIsBetweenDates(DateTime date, BorrowInfo[] borrowInfo) {
            foreach (BorrowInfo borrowRow in borrowInfo) {
                if (date <= borrowRow.DueDate.AddDays(1) && date >= borrowRow.BorrowDate) {
                    return true;
                }
            }
            return false;
        }
    }
}
