using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Tests
{
    public class EventTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public EventTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        private static string NewEmail() => $"{Guid.NewGuid()}@test.com";

        private static async Task<int> CreateEventAndGetId(HttpClient organiser)
        {
            var response = await organiser.PostAsJsonAsync("/api/events", TestHelpers.SampleEvent());
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("eventId").GetInt32();
        }

        [Fact]
        public async Task Organiser_CanCreateEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var response = await organiser.PostAsJsonAsync("/api/events", TestHelpers.SampleEvent());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CreateEvent_InvalidType_ReturnsBadRequest()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var bad = new
            {
                name = "Bad",
                description = "x",
                date = DateTime.UtcNow.AddDays(5),
                location = "Here",
                distanceKm = 5.0,
                eventType = "Swim"
            };
            var response = await organiser.PostAsJsonAsync("/api/events", bad);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanUpdateOwnEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(organiser);

            var response = await organiser.PutAsJsonAsync($"/api/events/{id}", TestHelpers.SampleEvent("Renamed Run"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanDeleteOwnEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(organiser);

            var response = await organiser.DeleteAsync($"/api/events/{id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotUpdateAnotherOrganisersEvent()
        {
            var owner = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var other = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(owner);

            var response = await other.PutAsJsonAsync($"/api/events/{id}", TestHelpers.SampleEvent("Stolen"));
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetEvents_NotLoggedIn_ReturnsUnauthorized()
        {
            var client = TestHelpers.NewClient(_factory);
            var response = await client.GetAsync("/api/events");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CanViewEvents()
        {
            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.GetAsync("/api/events");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}