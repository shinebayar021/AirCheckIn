
namespace Airport.Data.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public string PassportNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
