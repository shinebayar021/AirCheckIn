namespace Airport.Business.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }

        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = null!;

        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = null!;

        public int? SeatId { get; set; }
        public string? SeatNumber { get; set; }

        public bool CheckedIn { get; set; }

        public DateTime BookingTime { get; set; }
    }
}
