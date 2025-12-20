using System.Security.Cryptography;

namespace Shortly_API.Utils
{
    public static class ShortCodeGenerator
    {
        private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        // Configuración
        public const int DefaultLength = 8;
        public const int MaxLength = 10;

        public static string Generate(int? length = null)
        {
            int finalLength = length ?? DefaultLength;

            if (finalLength <= 0 || finalLength > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    $"Short code length must be between 1 and {MaxLength} characters."
                );
            }

            var bytes = RandomNumberGenerator.GetBytes(finalLength);
            var chars = new char[finalLength];

            for (int i = 0; i < finalLength; i++)
            {
                chars[i] = Alphabet[bytes[i] % Alphabet.Length];
            }

            return new string(chars);
        }
    }
}
