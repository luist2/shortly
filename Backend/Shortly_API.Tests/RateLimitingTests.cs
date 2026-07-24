using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Shortly_API.Tests;

public class RateLimitingTests
{
    [Fact]
    public async Task Login_keeps_the_strict_limit_of_five_requests_per_minute()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await client.PostAsync("/api/Auth/login", JsonContent());
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var rejectedResponse = await client.PostAsync("/api/Auth/login", JsonContent());
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
        Assert.NotNull(rejectedResponse.Headers.RetryAfter?.Delta);
    }

    [Fact]
    public async Task Refresh_allows_thirty_requests_before_returning_429_with_retry_after()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        for (var attempt = 0; attempt < 30; attempt++)
        {
            using var response = await client.PostAsync("/api/Auth/refresh-tokens", JsonContent());
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        using var rejectedResponse = await client.PostAsync("/api/Auth/refresh-tokens", JsonContent());
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
        Assert.NotNull(rejectedResponse.Headers.RetryAfter?.Delta);
        Assert.True(rejectedResponse.Headers.RetryAfter!.Delta!.Value.TotalSeconds > 0);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Testing"));

    private static StringContent JsonContent() =>
        new("{}", System.Text.Encoding.UTF8, "application/json");
}
