using Application.Abstraction.Parsing;
using Infrastructure.Implementation.Parsing;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IGuidFromBase64UrlConverter, GuidFromBase64UrlConverter>();

var provider = services.BuildServiceProvider();

var converter = provider.GetRequiredService<IGuidFromBase64UrlConverter>();

Console.Write("Enter Base64 URL string: ");
var input = Console.ReadLine();

if (converter.TryConvert(input, out var guid))
{
    Console.WriteLine($"Parsed Guid: {guid}");
}
else
{
    Console.WriteLine("Invalid Base64 URL string for Guid.");
}