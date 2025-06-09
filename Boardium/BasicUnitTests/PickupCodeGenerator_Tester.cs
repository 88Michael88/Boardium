using Boardium.HelperFuncs;
using Boardium.Models;

namespace BasicUnitTests {
    public class PickupCodeGenerator_Tester {
        PickupCodeGenerator _helperFuncs = new PickupCodeGenerator();
        public static string GenerateRandomString(int length) {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();
            char[] result = new char[length];
            for (int i = 0; i < length; i++) {
                result[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(result);
        }

        [Test]
        public void PickupCodeGenerator_ShouldReturnTrueForAll() {
            for (int i = 0; i < 1000; i++) {
                int result = _helperFuncs.GenerateCode(GenerateRandomString(i), DateTime.Now, i);
                Assert.That(result, Is.GreaterThan(0));
                Assert.That(result, Is.InRange(10_000_000, 99_999_999));
            }
        }
    }
}
