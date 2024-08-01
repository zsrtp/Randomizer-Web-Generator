namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TPRandomizer.Util;

    /// <summary>
    /// text.
    /// </summary>
    internal class Converter
    {
        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte GcByte(int x)
        {
            return (byte)x;
        }

        /// <summary>
        /// Returns x as BigEndian (GC).
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a Big Endian byte. </returns>
        public static byte[] GcBytes(UInt64 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(UInt32 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(UInt16 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(Int32 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="x">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte[] GcBytes(Int16 x)
        {
            var bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// Get bytes from text (without null terminator).
        /// </summary>
        /// <param name="text"> The ASCII text you want to convert.</param>
        /// <param name="desiredLength"> The length of the string in bytes. If
        /// not specified, returned array will match the length of the provided
        /// text.</param>
        /// <param name="region"> The language region of the text you want to
        /// convert. CURRENTLY UNUSED???</param>
        /// <returns>Array of Bytes processed.</returns>
        public static byte[] StringBytes(string text, int desiredLength = -1, char region = 'E')
        {
            List<byte> textData = new();

            if (desiredLength == 0 || text == null)
            {
                return new byte[0];
            }

            if (desiredLength < 0)
            {
                desiredLength = text.Length;
            }

            if (text.Length > desiredLength)
            {
                textData.AddRange(Encoding.ASCII.GetBytes(text.Substring(0, desiredLength)));
            }
            else
            {
                textData.AddRange(Encoding.ASCII.GetBytes(text));
            }

            // Account for padding
            while (textData.Count < desiredLength)
            {
                textData.Add(0);
            }

            return textData.ToArray<byte>();
        }

        /// <summary>
        /// Get bytes from text (without null terminator).
        /// </summary>
        /// <param name="text"> The ASCII text you want to convert.</param>
        /// <param name="desiredLength"> The length of the string in bytes.</param>
        /// <returns>Array of Bytes processed.</returns>
        public static byte[] MessageStringBytes(string text, int desiredLength = 0)
        {
            if (Res.IsCultureJa())
                return MessageStringBytesJa(text, desiredLength);

            // Windows-1252
            Encoding encoding = Encoding.GetEncoding(1252);

            List<byte> textData = new(encoding.GetBytes(text));

            // Account for padding
            while (textData.Count < desiredLength)
            {
                textData.Add(0);
            }

            return textData.ToArray<byte>();
        }

        private static byte[] MessageStringBytesJa(string text, int desiredLength = 0)
        {
            List<byte> bytes = new();

            if (!StringUtils.isEmpty(text))
            {
                // Shift-JIS
                Encoding encoding = Encoding.GetEncoding(932);

                int index = 0;
                while (index < text.Length)
                {
                    string currentChar = text.Substring(index, 1);

                    if (currentChar == "\x1A")
                    {
                        byte escLength = (byte)text[index + 1];

                        for (int escSeqIdx = 0; escSeqIdx < escLength; escSeqIdx++)
                        {
                            bytes.Add((byte)text[index + escSeqIdx]);
                        }

                        index += escLength;
                    }
                    else
                    {
                        bytes.AddRange(encoding.GetBytes(text.Substring(index, 1)));
                        index += 1;
                    }
                }
            }

            // Account for padding
            while (bytes.Count < desiredLength)
            {
                bytes.Add(0);
            }

            return bytes.ToArray<byte>();
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="text">The number you want to convert.</param>
        /// <returns> The inserted value as a byte. </returns>
        public static byte StringBytes(char text)
        {
            return (byte)text;
        }
    }
}
