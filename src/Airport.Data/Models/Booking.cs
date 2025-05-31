namespace Airport.Data.Models
{
    /// <summary>
    /// Booking: Зорчигч нислэг, суудлыг холбосон захиалга, бүртгэлийн төлөвтэй
    /// </summary>
    public class Booking
    {
        public int Id { get; set; }

        public int PassengerId { get; set; }
        public Passenger Passenger { get; set; } = null!;

        public int FlightId { get; set; }
        public Flight Flight { get; set; } = null!;

        public int? SeatId { get; set; }
        public Seat Seat { get; set; } = null!;

        public bool CheckedIn { get; set; } = false;

        public DateTime BookingTime { get; set; }
    }
}


