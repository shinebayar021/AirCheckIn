using Airport.Business.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airport.Business.Services.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingDto>> GetAllBookingsAsync();
    }
}
