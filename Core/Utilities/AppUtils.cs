using System.Text.Json;
using System.Text.RegularExpressions;

namespace Core.Utilities
{
    public interface IAppUtils
    {
        bool IsValidPassword(string password);
        bool IsValidPhoneNumber(string phoneNumber);
    }

    public class AppUtils: IAppUtils
    {
        /// <summary>
        /// Validates whether the input password meets the specified criteria:
        /// - Minimum length of 8 characters.
        /// - Contains at least one uppercase letter.
        /// - Contains at least one lowercase letter.
        /// - Contains at least one numeric digit.
        /// - Contains at least one special character (e.g., !@#$%^&*(),.?""{}|<>).
        /// </summary>
        /// <param name="password">The password string to validate.</param>
        /// <returns>True if the password meets all criteria; otherwise, false.</returns>
        public bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            const string pattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>]).{8,}$";

            return Regex.IsMatch(password, pattern);
        }

        /// <summary>
        /// Validates whether the input phone number string contains exactly 10 digits.
        /// </summary>
        /// <param name="phoneNumber">The phone number string to validate.</param>
        /// <returns>True if the phone number contains exactly 10 digits; otherwise, false.</returns>
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            const string pattern = @"^\+[1-9]\d{1,14}$";

            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
