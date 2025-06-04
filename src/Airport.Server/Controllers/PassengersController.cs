using Microsoft.AspNetCore.Mvc;
using Airport.Business.FlightServices.Interfaces;

namespace Airport.Server.Controllers
{
    //Зорчигчийн controller
    [ApiController]
    [Route("api/[controller]")]
    public class PassengersController : ControllerBase
    {
        private readonly IPassengerService _passengerService;

        public PassengersController(IPassengerService passengerService)
        {
            _passengerService = passengerService;
        }
        //бүх зорчигчид GET
        [HttpGet]
        public async Task<IActionResult> GetPassengers()
        {
            var passengers = await _passengerService.GetAllPassengersAsync();
            return Ok(passengers);
        }

        //зорчигч хайх GET
        [HttpGet("search")]
        public async Task<IActionResult> SearchByPassport(string passport)
        {
            if (string.IsNullOrEmpty(passport))
                return BadRequest("Passport dugaaraa oruul.");

            var result = await _passengerService.SearchPassengerByPassportAsync(passport);
            if (result == null)
                return NotFound($"Oldsongui {passport}");

            return Ok(result);
        }
        

    }
}
