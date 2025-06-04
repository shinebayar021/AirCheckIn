using Airport.Data;
using Airport.Business.DTOs;
using Airport.Data.Models;
using Microsoft.EntityFrameworkCore;
using Airport.Business.FlightServices.Interfaces;

namespace Airport.Business.Services
{
    public class SeatService : ISeatService
    {
        private readonly AirportDbContext _context;

        public SeatService(AirportDbContext context)
        {
            _context = context;
        }

        public async Task<List<Seat>> GetAllSeatsAsync()
        {
            return await _context.Seats.ToListAsync();
        }

        public async Task<List<Seat>?> GetSeatsByFlightNumberAsync(string flightNumber)
        {
            // Nislegee shuuj gargah
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
            if (flight == null)
                return null;
            // Shuusen nislegeer ni suudlaa gargah
            var seats = await _context.Seats.Where(s => s.FlightId == flight.FlightId).ToListAsync();
            return seats;
        }

        public async Task<(Seat? seat, string? error)> UpdateSeatStatusAsync(SeatUpdateDto updatedSeat)
        {
            //for api test
            if (updatedSeat == null || string.IsNullOrEmpty(updatedSeat.SeatNumber))
                return (null, "Bad request...");
            //seat key uusgeh, ene ni zereg adil suudal songoh processd ashiglana
            var seatKey = $"{updatedSeat.FlightId}_{updatedSeat.SeatNumber}";
            if (!await SeatLockManager.WaitToLockSeatAsync(seatKey))
                return (null, "Suudal odoogoor uur hereglegch songoj baina.");

            try
            {
                var seat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.SeatNumber == updatedSeat.SeatNumber && s.FlightId == updatedSeat.FlightId);

                if (seat == null)
                    return (null, "Suudal oldsongui.");

                if (seat.IsOccupied)
                    return (null, "Suudal ali hediin songoson baina.");

                seat.IsOccupied = updatedSeat.IsOccupied;
                seat.PassengerId = updatedSeat.PassengerId;

                await _context.SaveChangesAsync();

                return (seat, null);
            }
            finally
            {
                SeatLockManager.UnlockSeat(seatKey);
            }
        }

        public async Task<bool> HasPassengerCheckedInAsync(int passengerId)
        {
            return await _context.Seats.AnyAsync(s => s.PassengerId == passengerId && s.IsOccupied);
        }
    }
}
