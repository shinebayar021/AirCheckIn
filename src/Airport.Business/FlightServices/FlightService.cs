using Airport.Data;
using Airport.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Airport.Business.FlightServices.Interfaces;

namespace Airport.Business.Services
{
    public class FlightService : IFlightService
    {
        private readonly AirportDbContext _context;

        public FlightService(AirportDbContext context)
        {
            _context = context;
        }

        public async Task<List<Flight>> GetAllFlightsAsync()
        {
            return await _context.Flights.ToListAsync();
        }

        public async Task<Flight?> UpdateFlightStatusAsync(string flightNumber, FlightStatus newStatus)
        {
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
            if (flight == null) return null;

            flight.Status = newStatus;
            await _context.SaveChangesAsync();
            return flight;
        }
    }
}
