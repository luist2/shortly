using System.Text;

namespace Shortly_API.Utils
{
    public class Base62Converter
    {
        private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string ToBase62(long value)
        {
            if (value == 0) return Alphabet[0].ToString();

            var sb = new StringBuilder();
            while (value > 0)
            {
                sb.Insert(0, Alphabet[(int)(value % 62)]);
                value /= 62;
            }
            return sb.ToString();
        }
    }
}
