using System.Text;
using System.Security.Cryptography;
using Boardium.Models;

namespace Boardium.HelperFuncs {
    public class HelperFunctions {

        public bool DateIsBetweenDates(DateTime date, BorrowInfo[] borrowInfo) {
            foreach (BorrowInfo borrowRow in borrowInfo) {
                if (date <= borrowRow.DueDate.AddDays(1) && date >= borrowRow.BorrowDate.AddDays(-1)) {
                    return true;
                }
            }
            return false;
        }

        public int GenerateCode(string username, DateTime dateTime, int gameCopyID) {
            string combined = $"{username}-{dateTime:yyyyMMddHHmmss}-{gameCopyID}";

            using (SHA256 sha256 = SHA256.Create()) {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));

                int hashNumber = (int)BitConverter.ToUInt64(hashBytes, 0);

                int codeNumber = Math.Abs(hashNumber % 100_000_000);

                if (codeNumber < 10_000_000) {
                    codeNumber += 10_000_000;
                }

                return codeNumber;
            }
        }
    }
}
