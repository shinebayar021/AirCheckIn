using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airport.Data.Models;
using Airport.Business.DTOs;
using Airport.Data;

namespace Airport.Server.Controllers
{
    //Зорчигчийн controller
    [ApiController]
    [Route("api/[controller]")]
    public class PassengersController : ControllerBase
    {
        private readonly AirportDbContext _context;

        public PassengersController(AirportDbContext context)
        {
            _context = context;
        }
        //бүх зорчигчид GET
        [HttpGet]
        public async Task<IActionResult> GetPassengers()
        {
            var passengers = await _context.Passengers.ToListAsync();
            return Ok(passengers);
        }
        //зорчигч хайх GET
        [HttpGet("search")]
        public async Task<IActionResult> SearchByPassport(string passport)
        {
            if (string.IsNullOrEmpty(passport))
                return BadRequest("Passport number is required.");

            var passengerWithBookings = await _context.Passengers
                .Where(p => p.PassportNumber == passport)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Flight)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Seat)
                .FirstOrDefaultAsync();

            if (passengerWithBookings == null)
                return NotFound($"No passenger found with passport number {passport}");
            //??????????????????????????????????????????????????????????????????
            var result = new
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

            return Ok(result);
        }
        

    }
}
