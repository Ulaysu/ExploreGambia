using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace ExploreGambia.API.Models.Configurations
{
    public static class ForwardedHeadersConfiguration
    {
        public static void ConfigureTrustedSources(
            ForwardedHeadersOptions options,
            IConfiguration configuration)
        {
            // Empty lists keep ASP.NET Core's safe default behavior: forwarded
            // headers are ignored unless the immediate proxy is explicitly trusted.
            foreach (var knownProxy in configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
            {
                if (!IPAddress.TryParse(knownProxy, out var proxyAddress))
                {
                    throw new InvalidOperationException(
                        $"Invalid ForwardedHeaders:KnownProxies value '{knownProxy}'. Configure a valid IP address.");
                }

                options.KnownProxies.Add(proxyAddress);
            }

            foreach (var knownNetwork in configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
            {
                options.KnownNetworks.Add(ParseKnownNetwork(knownNetwork));
            }
        }

        private static Microsoft.AspNetCore.HttpOverrides.IPNetwork ParseKnownNetwork(string value)
        {
            // Deployment config uses CIDR strings such as "10.0.0.0/8"; parsing here
            // keeps appsettings simple while still validating prefix length bounds.
            var parts = value.Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var prefix))
            {
                throw new InvalidOperationException(
                    $"Invalid ForwardedHeaders:KnownNetworks value '{value}'. Configure a valid CIDR network address.");
            }

            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var prefixLength))
            {
                throw new InvalidOperationException(
                    $"Invalid ForwardedHeaders:KnownNetworks value '{value}'. Configure a valid CIDR prefix length.");
            }

            var maxPrefixLength = prefix.AddressFamily switch
            {
                System.Net.Sockets.AddressFamily.InterNetwork => 32,
                System.Net.Sockets.AddressFamily.InterNetworkV6 => 128,
                _ => throw new InvalidOperationException(
                    $"Invalid ForwardedHeaders:KnownNetworks value '{value}'. Configure an IPv4 or IPv6 network address.")
            };

            if (prefixLength < 0 || prefixLength > maxPrefixLength)
            {
                var addressType = maxPrefixLength == 32 ? "IPv4" : "IPv6";
                throw new InvalidOperationException(
                    $"Invalid ForwardedHeaders:KnownNetworks value '{value}'. {addressType} prefix length must be between 0 and {maxPrefixLength}.");
            }

            return new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength);
        }
    }
}
