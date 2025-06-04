using Airport.Business.FlightServices.Interfaces;
using Airport.Data;
using Airport.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Airport.Business.Services
{
    public class PassengerService : IPassengerService
    {
        private readonly AirportDbContext _context;

        public PassengerService(AirportDbContext context)
        {
            _context = context;
        }

        public async Task<List<Passenger>> GetAllPassengersAsync()
        {
            return await _context.Passengers.ToListAsync();
        }

        public async Task<object?> SearchPassengerByPassportAsync(string passport)
        {
            // passenger-aa bookingtai tsugt ni gargah
            var passengerWithBookings = await _context.Passengers
                .Where(p => p.PassportNumber == passport)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Flight)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Seat)
                .FirstOrDefaultAsync();

            if (passengerWithBookings == null)
                return null;
            //result-aa haruuldag bolgoh
            return new
            {
                PassengerId = passengerWithBookings.PassengerId,
                FullName = passengerWithBookings.FullName,
                PassportNumber = passengerWithBookings.PassportNumber,
                Bookings = passengerWithBookings.Bookings.Select(b => new
                {
                    BookingId = b.Id,
                    FlightNumber = b.Flight.FlightNumber,
                    DepartureTime = b.Flight.DepartureTime,
                    SeatNumber = b.Seat != null ? b.Seat.SeatNumber : null,
                    CheckedIn = b.CheckedIn
                }).ToList()
            };
        }
    }
}
