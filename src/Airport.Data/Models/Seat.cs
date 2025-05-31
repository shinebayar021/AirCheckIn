namespace Airport.Data.Models
{
    /// <summary>
    /// Seat ангилал нь нислэгийн суудлын мэдээллийг хадгална.
    /// </summary>
    public class Seat
    {
        /// <summary>
        /// Суудлын давтагдашгүй дугаар.
        /// </summary>
        public int SeatId { get; set; }

        /// <summary>
        /// Суудлын дугаар (жишээ нь: "12A").
        /// </summary>
        public string SeatNumber { get; set; } = string.Empty;

        /// <summary>
        /// Суудал эзэлсэн эсэх төлөв.
        /// </summary>
        public bool IsOccupied { get; set; }

        /// <summary>
        /// Энэ суудал ямар нислэгт хамаарахыг заана (нислэгийн ID).
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Энэ суудал ямар нислэгт хамаарахыг заана (нислэгийн мэдээлэл).
        /// </summary>
        public Flight Flight { get; set; } = null!;

        /// <summary>
        /// Энэ суудалд суусан зорчигчийн ID (null).
        /// </summary>
        public int? PassengerId { get; set; }

        /// <summary>
        /// Энэ суудалд суусан зорчигчийн мэдээлэл (null).
        /// </summary>
        public Passenger? Passenger { get; set; }
    }
}