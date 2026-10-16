using System.Net;

namespace RaceDay.Tests
{
    public class AuthTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public AuthTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        private static string NewEmail() => $"{Guid.NewGuid()}@test.com";

        [Fact]
        public async Task Register_ValidOrganiser_ReturnsOk()
        {
            var client = TestHelpers.NewClient(_factory);
            var response = await TestHelpers.Register(client, NewEmail(), "Organiser");
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        }
        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            var client = TestHelpers.NewClient(_factory);
            var response = await TestHelpers.Register(client, NewEmail(), "Admin");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var client = TestHelpers.NewClient(_factory);
            var email = NewEmail();
            await TestHelpers.Register(client, email, "Participant");
            var response = await TestHelpers.Register(client, email, "Participant");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_CorrectCredentials_ReturnsOk()
        {
            var client = TestHelpers.NewClient(_factory);
            var email = NewEmail();
            var reg = await TestHelpers.Register(client, email, "Participant");
            var regBody = await reg.Content.ReadAsStringAsync();
            var response = await TestHelpers.Login(client, email);
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, "REGISTER: " + reg.StatusCode + " " + regBody + " | LOGIN: " + body);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var client = TestHelpers.NewClient(_factory);
            var email = NewEmail();
            await TestHelpers.Register(client, email, "Participant");
            var response = await TestHelpers.Login(client, email, "WrongPassword");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Profile_NotLoggedIn_ReturnsUnauthorized()
        {
            var client = TestHelpers.NewClient(_factory);
            var response = await client.GetAsync("/api/profile");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Profile_LoggedIn_ReturnsOk()
        {
            var email = NewEmail();
            var client = await TestHelpers.LoggedInClient(_factory, email, "Participant");
            var response = await client.GetAsync("/api/profile");
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        }
    }
}