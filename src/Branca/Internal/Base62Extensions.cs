using System;
using System.IO;
using System.Text;

namespace Branca.Internal
{
    /// <summary>
    /// The base62 stream converter which is based on https://github.com/renmengye/base62-csharp library
    /// </summary>
    public static class Base62Extensions
    {
        private static string Base62CodingSpace = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string ToBase62(this byte[] original)
        {
            var sb = new StringBuilder();
            var stream = new BitStream(original); // Set up the BitStream
            var read = new byte[1]; // Only read 6-bit at a time
            while (true) 
            {
                read[0] = 0;
                var length = stream.Read(read, 0, 6); // Try to read 6 bits
                if (length == 6) // Not reaching the end
                {
                    switch (read[0] >> 3)
                    {
                        // First 5-bit is 11111
                        case 0x1f:
                            sb.Append(Base62CodingSpace[61]);
                            stream.Seek(-1, SeekOrigin.Current); // Leave the 6th bit to next group
                            break;
                        // First 5-bit is 11110
                        case 0x1e:
                            sb.Append(Base62CodingSpace[60]);
                            stream.Seek(-1, SeekOrigin.Current);
                            break;
                        // Encode 6-bit
                        default:
                            sb.Append(Base62CodingSpace[read[0] >> 2]);
                            break;
                    }
                }
                else if (length == 0) // Reached the end completely
                {
                    break;
                }
                else // Reached the end with some bits left
                {
                    // Padding 0s to make the last bits to 6 bit
                    sb.Append(Base62CodingSpace[read[0] >> 8 - length]);
                    break;
                }
            }

            return sb.ToString();
        }

        public static byte[] FromBase62(this string base62)
        {
            // Character count
            var count = 0;

            // Set up the BitStream
            var stream = new BitStream(base62.Length * 6 / 8);

            foreach (var c in base62)
            {
                // Look up coding table
                var index = Base62CodingSpace.IndexOf(c);

                // If end is reached
                if (count == base62.Length - 1)
                {
                    // Check if the ending is good
                    var mod = (int)(stream.Position % 8);
                    if (mod == 0)
                        throw new InvalidDataException("an extra character was found");

                    if ((index >> (8 - mod)) > 0)
                        throw new InvalidDataException("invalid ending character was found");

                    stream.Write(new byte[] {(byte)(index << mod)}, 0, 8 - mod);
                }
                else
                {
                    switch (index)
                    {
                        // If 60 or 61 then only write 5 bits to the stream, otherwise 6 bits.
                        case 60:
                            stream.Write(new byte[] {0xf0}, 0, 5);
                            break;
                        case 61:
                            stream.Write(new byte[] {0xf8}, 0, 5);
                            break;
                        default:
                            stream.Write(new byte[] {(byte)index}, 2, 6);
                            break;
                    }
                }

                count++;
            }

            // Dump out the bytes
            var result = new byte[stream.Position / 8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(result, 0, result.Length * 8);
            return result;
        }
    }

    public class BitStream : Stream
    {
        private byte[] Source { get; set; }

        public BitStream(int capacity)
        {
            Source = new byte[capacity];
        }

        public BitStream(byte[] source)
        {
            Source = source;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length => Source.Length * 8;

        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Temporary position cursor
            var tempPos = Position;
            tempPos += offset;

            // Buffer byte position and in-byte position
            int readPosCount = 0, readPosMod = 0;

            // Stream byte position and in-byte position
            var posCount = tempPos >> 3;
            var posMod = (int)(tempPos - ((tempPos >> 3) << 3));

            while (tempPos < Position + offset + count && tempPos < Length)
            {
                // Copy the bit from the stream to buffer
                if ((Source[posCount] & (0x1 << (7 - posMod))) != 0)
                {
                    buffer[readPosCount] = (byte)(buffer[readPosCount] | (0x1 << (7 - readPosMod)));
                }
                else
                {
                    buffer[readPosCount] = (byte)(buffer[readPosCount] & (0xffffffff - (0x1 << (7 - readPosMod))));
                }

                // Increment position cursors
                tempPos++;
                if (posMod == 7)
                {
                    posMod = 0;
                    posCount++;
                }
                else
                {
                    posMod++;
                }
                if (readPosMod == 7)
                {
                    readPosMod = 0;
                    readPosCount++;
                }
                else
                {
                    readPosMod++;
                }
            }
            var bits = (int)(tempPos - Position - offset);
            Position = tempPos;
            return bits;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case (SeekOrigin.Begin):
                    {
                        Position = offset;
                        break;
                    }
                case (SeekOrigin.Current):
                    {
                        Position += offset;
                        break;
                    }
                case (SeekOrigin.End):
                    {
                        Position = Length + offset;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Temporary position cursor
            var tempPos = Position;

            // Buffer byte position and in-byte position
            int readPosCount = offset >> 3, readPosMod = offset - ((offset >> 3) << 3);

            // Stream byte position and in-byte position
            var posCount = tempPos >> 3;
            var posMod = (int)(tempPos - ((tempPos >> 3) << 3));

            while (tempPos < Position + count && tempPos < Length)
            {
                // Copy the bit from buffer to the stream
                if ((buffer[readPosCount] & (0x1 << (7 - readPosMod))) != 0)
                {
                    Source[posCount] = (byte)(Source[posCount] | (0x1 << (7 - posMod)));
                }
                else
                {
                    Source[posCount] = (byte)(Source[posCount] & (0xffffffff - (0x1 << (7 - posMod))));
                }

                // Increment position cursors
                tempPos++;
                if (posMod == 7)
                {
                    posMod = 0;
                    posCount++;
                }
                else
                {
                    posMod++;
                }
                if (readPosMod == 7)
                {
                    readPosMod = 0;
                    readPosCount++;
                }
                else
                {
                    readPosMod++;
                }
            }
            Position = tempPos;
        }
    }
}
