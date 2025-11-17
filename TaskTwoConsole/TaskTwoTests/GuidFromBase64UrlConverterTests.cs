
using Application.Abstraction.Parsing;
using Infrastructure.Implementation.Parsing;

namespace TaskTwoTests
{
    public sealed class GuidFromBase64UrlConverterTests
    {
        private readonly IGuidFromBase64UrlConverter _converter = new GuidFromBase64UrlConverter();

        [Fact]
        public void TryConvert_ValidBase64Url_RoundTripsGuid()
        {
            // Arrange
            var original = Guid.NewGuid();
            var base64Url = ToBase64Url(original);

            // Act
            var ok = _converter.TryConvert(base64Url, out var parsed);

            // Assert
            Assert.True(ok);
            Assert.Equal(original, parsed);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryConvert_NullOrWhitespace_ReturnsFalse(string? input)
        {
            var ok = _converter.TryConvert(input, out var guid);

            Assert.False(ok);
            Assert.Equal(Guid.Empty, guid);
        }

        [Theory]
        [InlineData("a")]                    // length % 4 == 1, invalid
        [InlineData("abcde")]                // random, invalid
        [InlineData("****")]                 // invalid chars
        [InlineData("!!notbase64url!!")]     // invalid chars
        public void TryConvert_InvalidInput_ReturnsFalse(string input)
        {
            var ok = _converter.TryConvert(input, out var guid);

            Assert.False(ok);
            Assert.Equal(Guid.Empty, guid);
        }

        [Fact]
        public void TryConvert_ValidWithoutPadding_22Chars()
        {
            var original = Guid.NewGuid();

            // Normal Base64 (24 chars, includes ==)
            var base64 = Convert.ToBase64String(original.ToByteArray());

            // Remove '=' and make URL-safe.
            var base64NoPadding = base64.TrimEnd('=');
            var base64Url = base64NoPadding.Replace('+', '-')
                                           .Replace('/', '_');

            Assert.Equal(22, base64Url.Length);  // sanity check

            var ok = _converter.TryConvert(base64Url, out var parsed);

            Assert.True(ok);
            Assert.Equal(original, parsed);
        }

        [Fact]
        public void TryConvert_ValidWithPadding_24Chars()
        {
            var original = Guid.NewGuid();

            // 24-char base64
            var base64 = Convert.ToBase64String(original.ToByteArray());

            // URL-safe, keep padding
            var base64UrlWithPadding = base64.Replace('+', '-')
                                             .Replace('/', '_');

            Assert.Equal(24, base64UrlWithPadding.Length);

            var ok = _converter.TryConvert(base64UrlWithPadding, out var parsed);

            Assert.True(ok);
            Assert.Equal(original, parsed);
        }

        [Fact]
        public void TryConvert_IgnoresLeadingAndTrailingWhitespace()
        {
            var original = Guid.NewGuid();
            var base64Url = ToBase64Url(original);

            var input = $"  \t{base64Url}\r\n";

            var ok = _converter.TryConvert(input, out var parsed);

            Assert.True(ok);
            Assert.Equal(original, parsed);
        }

        // Helper: Guid -> URL-safe Base64 (22 chars) for testing
        private static string ToBase64Url(Guid guid)
        {
            var bytes = guid.ToByteArray();
            var base64 = Convert.ToBase64String(bytes);        // 24 chars
            var noPadding = base64.TrimEnd('=');
            return noPadding.Replace('+', '-').Replace('/', '_');
        }
    }
}
