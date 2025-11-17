using Application.Abstraction.Parsing;

namespace Infrastructure.Implementation.Parsing
{
    /// <summary>
    /// Memory allocation free converter from URL-safe Base64 string to Guid.
    /// </summary>
    public class GuidFromBase64UrlConverter : IGuidFromBase64UrlConverter
    {
        // Base64 for 16 bytes is 24 chars, with up to 2 '=' padding characters.
        private const int MaxBase64Length = 24;
        private const int GuidByteLength = 16;

        public bool TryConvert(string? base64Url, out Guid guid)
        {
            guid = Guid.Empty;

            if (string.IsNullOrWhiteSpace(base64Url))
                return false;

            ReadOnlySpan<char> input = base64Url.AsSpan().Trim(); // span (no new string)

            // URL-safe Base64 for GUID should typically be 22 (no padding) or 24 (with padding).
            if (input.Length is < 2 or > MaxBase64Length)
                return false;

            // Prepare buffers on the stack – no heap allocation.
            Span<char> base64Chars = stackalloc char[MaxBase64Length];
            Span<byte> bytes = stackalloc byte[GuidByteLength];

            // 1. Normalize URL-safe Base64 into standard Base64 chars.
            var written = NormalizeToStandardBase64(input, base64Chars);
            if (written == 0)
                return false;

            // 2. Add '=' padding if required.
            written = AddPadding(base64Chars, written);

            // 3. Decode Base64 directly into stackalloc'ed byte buffer.
            if (!Convert.TryFromBase64Chars(base64Chars[..written], bytes, out int bytesWritten))
                return false;

            if (bytesWritten != GuidByteLength)
                return false;

            // 4. Construct Guid from the 16-byte span.
            guid = new Guid(bytes);
            return true;
        }

        /// <summary>
        /// Converts URL-safe base64 chars to standard base64 chars into <paramref name="destination"/>.
        /// Returns number of chars written, or 0 if invalid.
        /// </summary>
        private static int NormalizeToStandardBase64(ReadOnlySpan<char> source, Span<char> destination)
        {
            if (source.Length > destination.Length)
                return 0;

            var written = 0;

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];

                // Map URL-safe characters to standard Base64.
                c = c switch
                {
                    '-' => '+',
                    '_' => '/',
                    _ => c
                };

                // Basic validation – reject whitespace in the middle, etc.
                if (char.IsWhiteSpace(c))
                    return 0;

                destination[written++] = c;
            }

            return written;
        }

        /// <summary>
        /// Adds '=' padding characters so that length is a multiple of 4.
        /// Returns new length; 0 indicates invalid.
        /// </summary>
        private static int AddPadding(Span<char> buffer, int length)
        {
            int mod = length % 4;

            // Length % 4 == 1 is never valid Base64.
            if (mod == 1)
                return 0;

            if (mod == 0)
                return length;

            int padding = 4 - mod;
            if (length + padding > buffer.Length)
                return 0;

            for (int i = 0; i < padding; i++)
            {
                buffer[length + i] = '=';
            }

            return length + padding;
        }
    }
}
