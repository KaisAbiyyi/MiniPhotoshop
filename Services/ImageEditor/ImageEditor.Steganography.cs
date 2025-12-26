using System;
using System.Text;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// LSB steganography operations for ImageEditor.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        private const int SteganographyHeaderBytes = 4; // Stores message length (Int32)

        /// <summary>
        /// Returns the maximum message length (in bytes) available for LSB embedding.
        /// Input: current image dimensions. Output: max bytes excluding header.
        /// Algorithm: capacityBits = width * height * 3, then subtract header (32 bits).
        /// </summary>
        public int GetMaxMessageLengthBytes()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            int width = State.OriginalBitmap.PixelWidth;
            int height = State.OriginalBitmap.PixelHeight;
            int capacityBits = width * height * 3;
            int capacityBytes = (capacityBits - (SteganographyHeaderBytes * 8)) / 8;
            return Math.Max(0, capacityBytes);
        }

        /// <summary>
        /// Embeds a UTF-8 message into the LSB of RGB channels.
        /// Input: message string. Output: new bitmap with embedded message.
        /// Algorithm: write message length (4 bytes) + payload bits into RGB LSBs.
        /// </summary>
        public BitmapSource EmbedMessage(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty.", nameof(message));
            }

            byte[] payload = Encoding.UTF8.GetBytes(message);
            int capacity = GetMaxMessageLengthBytes();
            if (payload.Length > capacity)
            {
                throw new InvalidOperationException($"Message too long. Max bytes: {capacity}.");
            }

            byte[] lengthBytes = BitConverter.GetBytes(payload.Length);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }

            byte[] fullPayload = new byte[SteganographyHeaderBytes + payload.Length];
            Array.Copy(lengthBytes, 0, fullPayload, 0, SteganographyHeaderBytes);
            Array.Copy(payload, 0, fullPayload, SteganographyHeaderBytes, payload.Length);

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            int totalBits = fullPayload.Length * 8;
            int bitIndex = 0;

            for (int i = 0; i < buffer.Length && bitIndex < totalBits; i += 4)
            {
                for (int channel = 0; channel < 3 && bitIndex < totalBits; channel++)
                {
                    int bit = GetPayloadBit(fullPayload, bitIndex);
                    buffer[i + channel] = (byte)((buffer[i + channel] & 0xFE) | bit);
                    bitIndex++;
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        /// <summary>
        /// Extracts a UTF-8 message from the LSB of RGB channels.
        /// Input: current image. Output: extracted message string.
        /// Algorithm: read 4-byte length header, then read that many payload bytes.
        /// </summary>
        public string ExtractMessage()
        {
            if (State.OriginalBitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            BitmapSource source = EnsureBgra32(State.OriginalBitmap);
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];
            source.CopyPixels(buffer, stride, 0);

            int capacityBytes = GetMaxMessageLengthBytes();
            int headerBits = SteganographyHeaderBytes * 8;
            byte[] lengthBytes = new byte[SteganographyHeaderBytes];

            for (int i = 0; i < headerBits; i++)
            {
                int bit = GetBufferBit(buffer, i);
                int byteIndex = i / 8;
                int bitOffset = 7 - (i % 8);
                lengthBytes[byteIndex] = (byte)(lengthBytes[byteIndex] | (bit << bitOffset));
            }

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }

            int length = BitConverter.ToInt32(lengthBytes, 0);
            if (length <= 0 || length > capacityBytes)
            {
                throw new InvalidOperationException("No valid message found in this image.");
            }

            int payloadBits = length * 8;
            byte[] payload = new byte[length];

            for (int i = 0; i < payloadBits; i++)
            {
                int bit = GetBufferBit(buffer, headerBits + i);
                int byteIndex = i / 8;
                int bitOffset = 7 - (i % 8);
                payload[byteIndex] = (byte)(payload[byteIndex] | (bit << bitOffset));
            }

            return Encoding.UTF8.GetString(payload);
        }

        private static int GetPayloadBit(byte[] payload, int bitIndex)
        {
            int byteIndex = bitIndex / 8;
            int bitOffset = 7 - (bitIndex % 8);
            return (payload[byteIndex] >> bitOffset) & 1;
        }

        private static int GetBufferBit(byte[] buffer, int bitIndex)
        {
            int pixelIndex = (bitIndex / 3) * 4;
            int channel = bitIndex % 3;
            if (pixelIndex + channel >= buffer.Length)
            {
                return 0;
            }

            return buffer[pixelIndex + channel] & 1;
        }
    }
}
