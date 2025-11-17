
namespace Application.Abstraction.Parsing
{
    public interface IGuidFromBase64UrlConverter
    {
        /// <summary>
        /// Tries to parse a URL-safe Base64 representation of a GUID into a <see cref="Guid"/>.
        /// The method is implemented to avoid heap allocations (uses stackalloc & spans).
        /// </summary>
        /// <param name="base64Url">URL-safe Base64 string (e.g. "A3qfTzF6Q0u9p0YV4f0a9w").</param>
        /// <param name="guid">Parsed Guid on success; Guid.Empty on failure.</param>
        /// <returns>True if parsing succeeded; otherwise false.</returns>
        bool TryConvert(string? base64Url, out Guid guid);
    }
}
