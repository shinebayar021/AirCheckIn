using System.Text.Json.Serialization;

namespace Airport.Data.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FlightStatus
    {
        Registering,    // Бүртгэж байна
        Boarding,       // Онгоцонд сууж байна
        Departed,       // Ниссэн
        Delayed,        // Хойшилсон
        Cancelled       // Цуцалсан
    }

    public class Flight
    {
        [JsonPropertyName("flightId")] // TODO
        public int FlightId { get; set; }
        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; } = null!;
        [JsonPropertyName("departureTime")]
        public DateTime DepartureTime { get; set; }
        [JsonPropertyName("status")]
        public FlightStatus Status { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
