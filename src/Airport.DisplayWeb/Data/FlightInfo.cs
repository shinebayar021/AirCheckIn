using System;
using System.Text.Json.Serialization;

namespace Airport.DisplayWeb.Data
{
    public class FlightInfo
    {
        // Хэрвээ API JSON-д "flightId" гэж ирж байвал харуулах, эсвэл ашиглахгүй бол устгаад болно
        [JsonPropertyName("flightId")]
        public int FlightId { get; set; }

        // JSON-д ирж буй "flightNumber"
        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; } = string.Empty;

        // JSON-д ирж буй "departureTime"
        [JsonPropertyName("departureTime")]
        public DateTime DepartureTime { get; set; }

        // JSON-д ирж буй "status"
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // Хэрвээ API JSON-д "bookings" гэж ирж байгаа боловч энд ашиглахгүй бол дараах талбарыг устгах эсвэл хэвээр үлдээж болно
        [JsonPropertyName("bookings")]
        public object[] Bookings { get; set; } = Array.Empty<object>();
    }
}
