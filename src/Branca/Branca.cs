using System;
using System.Security.Cryptography;
using Branca.Internal;
using NaCl.Core;
using NaCl.Core.Base;

namespace Branca
{
    /// <summary>
    /// Specification at https://github.com/tuupola/branca-spec
    /// Version (1B) || Timestamp (4B) || Nonce (24B) || Ciphertext (*B) || Tag (16B)
    /// </summary>
    public class Branca
    {
        private const string Version = "BA";
        private const int KeyBytes = 32;
        private const int VersionByte = 1;
        private const int TimestampBytes = 4;
        private const int NonceBytes = 12;
        private const int HeaderBytes = VersionByte + TimestampBytes + NonceBytes;

        private readonly string _key;

        public Branca(string key)
        {
            if (KeyBytes != key.Length)
            {
                throw new ArgumentException($"Key must be exactly {KeyBytes} bytes.");
            }

            _key = key;
        }

        public string Encode(string payload, int? timestamp = null)
        {
            if (!timestamp.HasValue)
            {
                timestamp = (int)DateTime.UtcNow.Ticks;
            }

            var versionByte = new byte[VersionByte];
            Buffer.BlockCopy(Version.ToCharArray(), 0, versionByte, 0, VersionByte);

            var nonceBytes = new byte[NonceBytes];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(nonceBytes);

            var timestampBytes = new byte[TimestampBytes];
            Buffer.BlockCopy(timestamp.ToString().ToCharArray(), 0, timestampBytes, 0, timestampBytes.Length);

            var headerBytes = new byte[HeaderBytes];
            Buffer.BlockCopy(versionByte, 0, headerBytes, 0, VersionByte);
            Buffer.BlockCopy(timestampBytes, 0, headerBytes, VersionByte, TimestampBytes);
            Buffer.BlockCopy(nonceBytes, 0, headerBytes, VersionByte + TimestampBytes, NonceBytes);

            var plainTextBytes = new byte[payload.GetBytes().Length];
            Buffer.BlockCopy(payload.GetBytes(), 0, plainTextBytes, 0, plainTextBytes.Length);

            var keyBytes = new byte[Snuffle.KEY_SIZE_IN_BYTES];
            Buffer.BlockCopy(_key.ToCharArray(), 0, keyBytes, 0, _key.ToCharArray().Length);

            var aead = new ChaCha20Poly1305(keyBytes);
            var cipherTextBytes = aead.Encrypt(plainTextBytes, null, nonceBytes);

            var tokenBytes = new byte[headerBytes.Length + cipherTextBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, tokenBytes, 0, headerBytes.Length);
            Buffer.BlockCopy(cipherTextBytes, 0, tokenBytes, headerBytes.Length, cipherTextBytes.Length);

            return tokenBytes.ToBase62();
        }

        public string Decode(string token, int? ttl = null)
        {
            var tokenBytes = token.FromBase62();

            var headerBytes = new byte[HeaderBytes];
            Buffer.BlockCopy(tokenBytes, 0, headerBytes, 0, HeaderBytes);

            var nonceBytes = new byte[NonceBytes];
            Buffer.BlockCopy(headerBytes, VersionByte + TimestampBytes, nonceBytes, 0, NonceBytes);

            var cipherBytes = new byte[tokenBytes.Length - HeaderBytes];
            Buffer.BlockCopy(tokenBytes, headerBytes.Length, cipherBytes, 0, cipherBytes.Length);

            var keyBytes = new byte[Snuffle.KEY_SIZE_IN_BYTES];
            Buffer.BlockCopy(_key.ToCharArray(), 0, keyBytes, 0, _key.ToCharArray().Length);

            var aead = new ChaCha20Poly1305(keyBytes);
            var cipherText = aead.Decrypt(cipherBytes, null, nonceBytes);

            return cipherText.GetString();
        }

        /*public uint TimeStamp(string token)
        {
            var tokenBytes = token.FromBase62();

            // will do it better like https://stackoverflow.com/questions/879722/what-is-the-idiomatic-c-sharp-for-unpacking-an-integer-from-a-byte-array
            return tokenBytes[TimestampBytes];
        }*/
    }
}
