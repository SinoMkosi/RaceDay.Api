using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Tests
{
    public class RoleAccessTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public RoleAccessTests(TestWebAppFactory factory)
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
        public async Task Participant_CannotCreateEvent()
        {
            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.PostAsJsonAsync("/api/events", TestHelpers.SampleEvent());
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotDeleteEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.DeleteAsync($"/api/events/{id}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotAddCategory()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.PostAsJsonAsync($"/api/events/{id}/categories", new { name = "Senior" });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotEnrol()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(organiser);

            var response = await organiser.PostAsJsonAsync($"/api/events/{id}/enrol", new { categoryId = 1 });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotViewEnrolmentsForEvent()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var id = await CreateEventAndGetId(organiser);

            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.GetAsync($"/api/events/{id}/enrolments");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CannotViewMyResults()
        {
            var organiser = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Organiser");
            var response = await organiser.GetAsync("/api/results/mine");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotCaptureResult()
        {
            var participant = await TestHelpers.LoggedInClient(_factory, NewEmail(), "Participant");
            var response = await participant.PostAsJsonAsync("/api/results",
                new { enrolmentId = 1, finishTime = "00:45:00", position = 1 });
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}