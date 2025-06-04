using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Airport.Data.Models;

namespace Airport.Business.FlightServices.Interfaces
{
    public interface IPassengerService
    {
        Task<List<Passenger>> GetAllPassengersAsync();
        Task<object?> SearchPassengerByPassportAsync(string passport);
    }
}
