using Shortly_API.Utils;

namespace Shortly_API.Services
{
    public class ShortCodeGenerator
    {
        public static string Generate(int length = 8)
        {
            // Crear un GUID y tomar sus bytes
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();

            // Convertir parte del GUID en long
            long value = BitConverter.ToInt64(bytes, 0);

            // Pasar a Base62
            string base62 = Base62Converter.ToBase62(Math.Abs(value));

            // Recortar al tamaño deseado
            return base62.Substring(0, Math.Min(length, base62.Length));
        }
    }
}
