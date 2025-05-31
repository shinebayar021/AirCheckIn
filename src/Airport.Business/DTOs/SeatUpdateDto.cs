using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airport.Business.DTOs
{
    public class SeatUpdateDto
    {
        public string SeatNumber { get; set; }
        public int FlightId { get; set; }
        public int? PassengerId { get; set; }
        public bool IsOccupied { get; set; }
    }

}
