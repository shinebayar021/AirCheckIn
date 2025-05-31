using Microsoft.AspNetCore.Mvc;
using Airport.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Airport.Business.DTOs;
using Airport.Data.Models;
using Airport.Hubs;
using Microsoft.AspNetCore.SignalR;
namespace Airport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly AirportDbContext _context;
        private readonly IHubContext<SeatsHub> _hubContext;

        public FlightsController(AirportDbContext context, IHubContext<SeatsHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // All flights GET
        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            var flights = await _context.Flights.ToListAsync();
            return Ok(flights);
        }

        // Flight status UPDATE
        [HttpPut("update-status/{flightNumber}")]
        public async Task<IActionResult> UpdateFlightStatus(string flightNumber, [FromBody] FlightStatus newStatus)
        {
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
            if (flight == null)
                return NotFound(new { message = $"Flight '{flightNumber}' ?????????." });

            flight.Status = newStatus;
            await _context.SaveChangesAsync();

            // Real-time ???????? ??????
            // ???????????? flight ???????? ???????? ?? DTO ????? ??????? ?????? ?????.
            var updatedDto = new
            {
                flight.FlightNumber,
                flight.Status,
                flight.DepartureTime,
                // ... ???????????? ????? ?????????
            };

            await _hubContext.Clients.All.SendAsync("ReceiveFlightUpdate", updatedDto);

            return Ok(new { message = "Flight ?????? ????????? ????????????." });
        }
    }

// Seats controller
[ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly AirportDbContext _context;

        public SeatsController(AirportDbContext context)
        {
            _context = context;
        }

        //Buh ongotsnii buh suudal
        [HttpGet]
        public async Task<IActionResult> GetSeats()
        {
            var seats = await _context.Seats.ToListAsync();
            return Ok(seats);
        }

        //Suudlaa nislegeer ni shuuj gargah
        [HttpGet("by-flight/{flightNumber}")]
        public async Task<IActionResult> GetSeatsByFlight(string flightNumber)
        {
            // Flight-??? flightNumber-??? ????
            var flight = await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

            if (flight == null)
                return NotFound($"Flight number {flightNumber} ?????????.");

            // ?????? Flight-??? ID-? Seats ????
            var seats = await _context.Seats
                .Where(s => s.FlightId == flight.FlightId)
                .ToListAsync();

            if (seats == null)
                return NotFound($"Flight number {flightNumber} ???? ?????? ?????????.");

            return Ok(seats);
        }

        //Suudal ezlegdsen bolgoh (check in)
        [HttpPut("seat-status")]
        public async Task<IActionResult> UpdateSeat([FromBody] SeatUpdateDto updatedSeat)
        {
            if (updatedSeat == null || string.IsNullOrEmpty(updatedSeat.SeatNumber))
                return BadRequest("Invalid seat data.");

            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.SeatNumber == updatedSeat.SeatNumber && s.FlightId == updatedSeat.FlightId);

            if (seat == null)
                return NotFound("Seat not found.");

            seat.IsOccupied = updatedSeat.IsOccupied;
            seat.PassengerId = updatedSeat.PassengerId;

            await _context.SaveChangesAsync();

            return Ok(seat);
        }
        // ??????? ??? ?????? ?????? ???????? ??????? ??????
        [HttpGet("checked-in/{passengerId}")]
        public async Task<IActionResult> HasPassengerCheckedIn(int passengerId)
        {
            var hasSeat = await _context.Seats.AnyAsync(s => s.PassengerId == passengerId && s.IsOccupied);
            return Ok(hasSeat);
        }

    }

    //booking controller
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly AirportDbContext _context;

        public BookingsController(AirportDbContext context)
        {
            _context = context;
        }
        //booking GET
        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings
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

            return Ok(bookings);
        }

        //[HttpPut]
        //public async Task<IActionResult> UpdateBooking()
        //{
        //    return NotFound;
        //}

    }
}
