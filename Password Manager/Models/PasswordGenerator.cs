using System;
using System.Text;

namespace Password_Manager.Models
{
    public static class PasswordGenerator
    {
        private static readonly Random _random = new Random();

        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Symbols = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        // Force the parameters, but I may change this in the future
        public static string Generate(int length = 24, bool useUppercase = true, bool useLowercase = true, bool useDigits = true, bool useSymbols = true)
        {
            // Check if the requested password length is valid
            if (length <= 0)
                throw new ArgumentException("Password length must be greater than zero.");

            // Adds all the characters together depending on the bool
            var charPool = new StringBuilder();
            if (useUppercase) charPool.Append(Uppercase);
            if (useLowercase) charPool.Append(Lowercase);
            if (useDigits) charPool.Append(Digits);
            if (useSymbols) charPool.Append(Symbols);

            // Check if the character pool has at least one type selected
            if (charPool.Length == 0)
                throw new ArgumentException("At least one character set must be selected.");

            // Character array to hold the password
            var password = new char[length];
            for (int i = 0; i < length; i++)
            {
                // Pick a random character from the pool and assign it to password until the end of the length has been reached
                password[i] = charPool[_random.Next(charPool.Length)];
            }

            // Convert the character array to a string and return the value the password
            return new string(password);
        }
    }

}
