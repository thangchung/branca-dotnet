using System;
using System.Text;

namespace Branca.Internal
{
    internal static class HexExtensions
    {
        public static string ToHexString(this string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string FromHexString(this string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }

        public static string Encode(this byte[] raw)
        {
            return BitConverter.ToString(raw).Replace("-", string.Empty).ToLowerInvariant();
        }

        public static byte[] Decode(this string hex)
        {
            var raw = new byte[hex.Length / 2];

            for (var i = 0; i < raw.Length; ++i)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return raw;
        }
    }
}
