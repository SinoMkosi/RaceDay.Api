using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Tests
{
    public class EnrolmentTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public EnrolmentTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        private static string NewEmail() => $"{Guid.NewGuid()}@test.com";

        // Creates an event and a category as an Organiser. Returns both ids.
        private static async Task<(int eventId, int categoryId)> SetupEventWithCategory(HttpClient organiser)
        {
            var evResponse = await organiser.PostAsJsonAsync("/api/events", TestHelpers.SampleEvent());
            var evJson = await evResponse.Content.ReadFromJsonAsync<JsonElement>();
            var eventId = evJson.GetProperty("eventId").GetInt32();

            var catResponse = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories", new { name = "10km" });
            var catJson = await catResponse.Content.ReadFromJsonAsync<JsonElement>();
            var categoryId = catJson.GetProperty("categoryId").GetInt32();

            return (eventId, categoryId);
        }

        [Fact]
        public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var (eventId, categoryId) = await SetupEventWithCategory(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/enrol", new { categoryId });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(eventId, json.GetProperty("eventId").GetInt32());
            Assert.Equal(categoryId, json.GetProperty("categoryId").GetInt32());

            var mine = await participant.GetAsync("/api/enrolments/mine");
            var list = await mine.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
        }

        [Fact]
        public async Task Participant_CannotEnrolTwice()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var (eventId, categoryId) = await SetupEventWithCategory(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            await participant.PostAsJsonAsync($"/api/events/{eventId}/enrol", new { categoryId });
            var second = await participant.PostAsJsonAsync($"/api/events/{eventId}/enrol", new { categoryId });

            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotEnrol_WithCategoryFromAnotherEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var (eventId, _) = await SetupEventWithCategory(organiser);
            var (_, otherCategoryId) = await SetupEventWithCategory(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/enrol",
                new { categoryId = otherCategoryId });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task NotLoggedIn_CannotEnrol()
        {
            var client = TestHelpers.NewClient(_factory);
            var response = await client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanViewEnrolmentsForOwnEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var (eventId, categoryId) = await SetupEventWithCategory(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            await participant.PostAsJsonAsync($"/api/events/{eventId}/enrol", new { categoryId });

            var response = await organiser.GetAsync($"/api/events/{eventId}/enrolments");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var list = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
        }
    }
}