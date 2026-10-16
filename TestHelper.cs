using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RaceDay.Tests
{
    public static class TestHelpers
    {
        public const string Password = "Pass123!";

        public static HttpClient NewClient(TestWebAppFactory factory) =>
            factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

        public static Task<HttpResponseMessage> Register(HttpClient client, string email, string role) =>
            client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName = "Test",
                lastName = "User",
                email,
                password = Password,
                role
            });

        public static Task<HttpResponseMessage> Login(HttpClient client, string email, string password = Password) =>
            client.PostAsJsonAsync("/api/auth/login", new { email, password });

        public static async Task<HttpClient> LoggedInClient(TestWebAppFactory factory, string email, string role)
        {
            var client = NewClient(factory);
            await Register(client, email, role);
            await Login(client, email);
            return client;
        }

        public static object SampleEvent(string name = "City Run") => new
        {
            name,
            description = "A fun run",
            date = DateTime.UtcNow.AddDays(30),
            location = "Cape Town",
            distanceKm = 10.0,
            eventType = "Run"
        };
    }
}
