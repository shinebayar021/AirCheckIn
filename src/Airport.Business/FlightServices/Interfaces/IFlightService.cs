using Airport.Data.Models;

namespace Airport.Business.FlightServices.Interfaces
{
    public interface IFlightService
    {
        Task<List<Flight>> GetAllFlightsAsync();
        Task<Flight?> UpdateFlightStatusAsync(string flightNumber, FlightStatus newStatus);
    }
}
