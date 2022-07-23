namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
        /// <param name="region"> The language region of the text you want to convert.</param>
        /// <returns>Array of Bytes processed.</returns>
        public static byte[] MessageStringBytes(
            string text,
            int desiredLength = 0,
            char region = 'E'
        )
        {
            List<byte> textData = new();
            for (int i = 0; i < text.Length; i++)
            {
                textData.Add((byte)text[i]);
            }

            // Account for padding
            while (textData.Count < desiredLength)
            {
                textData.Add(0);
            }

            return textData.ToArray<byte>();
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
