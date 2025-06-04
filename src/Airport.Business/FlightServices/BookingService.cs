using Airport.Data;
using Airport.Business.DTOs;
using Microsoft.EntityFrameworkCore;
using Airport.Business.Services.Interfaces;

namespace Airport.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly AirportDbContext _context;

        public BookingService(AirportDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookingDto>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .Include(b => b.Seat)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    CheckedIn = b.CheckedIn,
                    FlightId = b.FlightId,
                    FlightNumber = b.Flight.FlightNumber,
                    PassengerId = b.PassengerId,
                    PassengerName = b.Passenger.FullName,
                    SeatId = b.SeatId,
                    SeatNumber = b.Seat != null ? b.Seat.SeatNumber : null,
                    BookingTime = b.BookingTime
                })
                .ToListAsync();
        }
    }
}
