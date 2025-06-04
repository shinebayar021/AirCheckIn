using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Airport.Business.DTOs;
using Airport.Data.Models;

namespace Airport.Business.FlightServices.Interfaces
{
    public interface ISeatService
    {
        Task<List<Seat>> GetAllSeatsAsync();
        Task<List<Seat>?> GetSeatsByFlightNumberAsync(string flightNumber);
        Task<(Seat? seat, string? error)> UpdateSeatStatusAsync(SeatUpdateDto updatedSeat);
        Task<bool> HasPassengerCheckedInAsync(int passengerId);
    }
}
