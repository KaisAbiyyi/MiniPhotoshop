using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Services.Contracts
{
    /// <summary>
    /// Provides LSB steganography operations for embedding and extracting messages.
    /// </summary>
    public interface ISteganographyService
    {
        /// <summary>
        /// Returns the maximum message length (in bytes) that can be embedded.
        /// </summary>
        int GetMaxMessageLengthBytes();

        /// <summary>
        /// Embeds the provided message into the current image using LSB.
        /// </summary>
        BitmapSource EmbedMessage(string message);

        /// <summary>
        /// Extracts a message from the current image using LSB.
        /// </summary>
        string ExtractMessage();
    }
}
