using Microsoft.EntityFrameworkCore;
using Airport.Data.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Airport.Data
{
    public class AirportDbContext : DbContext
    {
        public AirportDbContext(DbContextOptions<AirportDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; } = null!;
        public DbSet<Passenger> Passengers { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Seat> Seats { get; set; } = null!;

        public void SeedData()
        {
            // ugugdul hervee baival dahin nemeh shaardlagagui
            if (Flights.Any())
                return;

            // Nisleg dummy
            var flight1 = new Flight
            {
                FlightNumber = "MN100",
                DepartureTime = new DateTime(2025, 6, 1, 8, 0, 0),
                Status = FlightStatus.Registering
            };
            var flight2 = new Flight
            {
                FlightNumber = "MN200",
                DepartureTime = new DateTime(2025, 6, 2, 12, 0, 0),
                Status = FlightStatus.Boarding
            };
            Flights.AddRange(flight1, flight2);
            SaveChanges();

            // Zorchigch dummy
            var passenger1 = new Passenger { PassportNumber = "AB1234567", FullName = "Bat Erdene" };
            var passenger2 = new Passenger { PassportNumber = "CD7654321", FullName = "Sara Bold" };
            var passenger3 = new Passenger { PassportNumber = "EF1122334", FullName = "Tsetseg Naran" };
            Passengers.AddRange(passenger1, passenger2, passenger3);
            SaveChanges();

            // Suudal dummy
            string[] letters = { "A", "B", "C", "D", "E", "F" };
            var seats = new List<Seat>();
            foreach (var flight in new[] { flight1, flight2 })
            {
                for (int row = 1; row <= 30; row++)
                {
                    foreach (var letter in letters)
                    {
                        seats.Add(new Seat
                        {
                            SeatNumber = $"{row}{letter}",
                            FlightId = flight.FlightId
                        });
                    }
                }
            }
            Seats.AddRange(seats);
            SaveChanges();

            // Jishee zahialguud uusgeh
            var seat1 = seats.First(s => s.FlightId == flight1.FlightId && s.SeatNumber == "1A");
            var seat2 = seats.First(s => s.FlightId == flight1.FlightId && s.SeatNumber == "1B");

            var booking1 = new Booking { FlightId = flight1.FlightId, PassengerId = passenger1.PassengerId, SeatId = seat1.SeatId, CheckedIn = false };
            var booking2 = new Booking { FlightId = flight1.FlightId, PassengerId = passenger2.PassengerId, SeatId = seat2.SeatId, CheckedIn = true };
            var booking3 = new Booking { FlightId = flight2.FlightId, PassengerId = passenger3.PassengerId, SeatId = null, CheckedIn = false };

            Bookings.AddRange(booking1, booking2, booking3);
            SaveChanges();

            // Shine zorchigch dummy for testing
            var newPassenger = new Passenger
            {
                PassportNumber = "GH9988776",
                FullName = "Ganaa Nyamdorj"
            };
            Passengers.Add(newPassenger);
            SaveChanges();

            var freeSeat = seats
                .Where(s => s.FlightId == flight1.FlightId && !Bookings.Any(b => b.SeatId == s.SeatId))
                .FirstOrDefault();

            if (freeSeat != null)
            {
                var newBooking = new Booking
                {
                    FlightId = flight1.FlightId,
                    PassengerId = newPassenger.PassengerId,
                    SeatId = freeSeat.SeatId,
                    CheckedIn = false
                };
                Bookings.Add(newBooking);
                SaveChanges();
            }

            Console.WriteLine("Seeded Flights and Seats:");
            foreach (var f in Flights)
            {
                Console.WriteLine($"Flight {f.FlightNumber} -d {Seats.Count(s => s.FlightId == f.FlightId)} suudluud baigaa.");
            }
        }
    }
}
