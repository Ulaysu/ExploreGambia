using System.Net;
using ExploreGambia.API.Models.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ExploreGambia.API.Tests.RateLimiting
{
    public class ForwardedHeadersConfigurationTests
    {
        [Fact]
        public void ConfigureTrustedSources_WithValidProxy_AddsKnownProxy()
        {
            var options = new ForwardedHeadersOptions();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownProxies:0"] = "10.0.0.10"
            });

            ForwardedHeadersConfiguration.ConfigureTrustedSources(options, configuration);

            Assert.Contains(IPAddress.Parse("10.0.0.10"), options.KnownProxies);
        }

        [Fact]
        public void ConfigureTrustedSources_WithMalformedProxyIp_ThrowsClearException()
        {
            var options = new ForwardedHeadersOptions();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownProxies:0"] = "not-an-ip-address"
            });

            var exception = Assert.Throws<InvalidOperationException>((Action)(() =>
                ForwardedHeadersConfiguration.ConfigureTrustedSources(options, configuration)));

            Assert.Contains("not-an-ip-address", exception.Message);
            Assert.Contains("ForwardedHeaders:KnownProxies", exception.Message);
        }

        [Fact]
        public void ConfigureTrustedSources_WithValidNetwork_AddsKnownNetwork()
        {
            var options = new ForwardedHeadersOptions();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownNetworks:0"] = "10.0.0.0/8"
            });

            ForwardedHeadersConfiguration.ConfigureTrustedSources(options, configuration);

            Assert.Contains(
                options.KnownNetworks,
                network => network.Prefix.Equals(IPAddress.Parse("10.0.0.0")) &&
                    network.PrefixLength == 8);
        }

        [Fact]
        public void ConfigureTrustedSources_WithMalformedNetworkAddress_ThrowsClearException()
        {
            var options = new ForwardedHeadersOptions();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownNetworks:0"] = "not-a-network/24"
            });

            var exception = Assert.Throws<InvalidOperationException>((Action)(() =>
                ForwardedHeadersConfiguration.ConfigureTrustedSources(options, configuration)));

            Assert.Contains("not-a-network/24", exception.Message);
            Assert.Contains("ForwardedHeaders:KnownNetworks", exception.Message);
        }

        [Fact]
        public void ConfigureTrustedSources_WithInvalidIpv4Prefix_ThrowsClearException()
        {
            var options = new ForwardedHeadersOptions();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownNetworks:0"] = "10.0.0.0/33"
            });

            var exception = Assert.Throws<InvalidOperationException>((Action)(() =>
                ForwardedHeadersConfiguration.ConfigureTrustedSources(options, configuration)));

            Assert.Contains("10.0.0.0/33", exception.Message);
            Assert.Contains("between 0 and 32", exception.Message);
        }

        [Fact]
        public void ConfigureTrustedSources_WithInvalidIpv6Prefix_ThrowsClearException()
        {
            var options = new ForwardedHeadersOptions();
            var configuration = CreateConfiguration(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownNetworks:0"] = "2001:db8::/129"
            });

            var exception = Assert.Throws<InvalidOperationException>((Action)(() =>
                ForwardedHeadersConfiguration.ConfigureTrustedSources(options, configuration)));

            Assert.Contains("2001:db8::/129", exception.Message);
            Assert.Contains("between 0 and 128", exception.Message);
        }

        private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}
