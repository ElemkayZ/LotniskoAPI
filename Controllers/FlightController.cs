using System.Net.Http;
using System.Text.Json;
using System.Text;
using LotniskoAPI.Data;
using LotniskoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace LotniskoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    //[Authorize]

    public class FlightController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlightController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint GET: api/Flight
        [HttpGet("AllFlights")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            try
            {
                var flights = await _context.Flights
                    .Include(f => f.Status)
                    .ToListAsync();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }
        // Endpoint wyszukiwania
       

        [HttpGet("GetAllPlanes")]
        public async Task<ActionResult<List<Flight>>> GetAllPlanes()
        {
            try
            {
                var flights = await _context.Planes
                    .ToListAsync();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }

        // Endpoint GET: api/FlightAndStatus
        [HttpGet("OneFlightAndStatus")]
        public async Task<ActionResult<FlightAndStatus>> GetFlightAndStatus([FromQuery] int id)
        {
            try
            {
                var flight = await _context.Flights
                .FirstOrDefaultAsync(xd => xd.Id == id);

                if (flight == null)
                {
                    return NotFound();
                }

                var flightStatus = _context.FlightStatuses.FirstOrDefault(f => f.Id == id);
                if (flightStatus == null)
                {
                    return NotFound();
                }
                FlightAndStatus fas = new FlightAndStatus() { _Flight = flight, _FlightStatus = flightStatus };
                return fas;

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Flight>>>
            GetAvailableFlightsToWarsaw([FromQuery] string destination, [FromQuery] DateTime dTime, [FromQuery] string rodzajLotu)
        {
            try
            {
                var results = await _context.Flights
                .Include(f => f.Status) // Load FlightStatus
                .Where(f => f.Destination == destination &&
                        f.Status != null &&
                        f.AvailableSeats > 0 &&
                        f.Status.DepartureTime >= dTime && 
                        f.Type == rodzajLotu)
                .OrderBy(f => f.Id)
                .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.",
                    Details = ex.Message });
            }
        }

        [HttpGet("GetPlane")]
        public async Task<ActionResult<Plane>> GetPlane([FromQuery] int id)
        {
            try
            {
                var Plane = await _context.Planes
                .FirstOrDefaultAsync(f => f.Id == id);

                if (Plane == null)
                {
                    return NotFound();
                }
                return Ok(Plane);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }
        [HttpGet("GetAllPlane")]
        public async Task<ActionResult<IEnumerable<Plane>>> GetAllPlane()
        {
            try
            {
                var Plane = await _context.Planes
                    .OrderBy(f => f.Id)
                .ToListAsync();

                if (Plane == null)
                {
                    return NotFound();
                }
                return Ok(Plane);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }
        public class AddFlightRequ
        {
            public int _Id { get; set; }
            public string _Starting { get; set; }
            public string _Destination { get; set; }
            public string Type { get; set; }
            public int _Distance { get; set; }
            public DateTime _DepartureTime { get; set; }
            public DateTime _ArrivalTime { get; set; }
            public int _Terminal { get; set; }
            public int _Gate { get; set; }
            public DateTime _CheckinTime { get; set; }
            public int _BaggageCarousel { get; set; }
        }
        [HttpPost("addFlight")]
        public async Task<IActionResult> AddFlight([FromBody] AddFlightRequ _flight
            //[FromQuery] int _CurrentPlaneId,
            //[FromQuery] string _Starting,
            //[FromQuery] string _Destination,
            //[FromQuery] string _Type,
            //[FromQuery] int _Distance,
            //[FromQuery] DateTime _DepartureTime,
            //[FromQuery] DateTime _ArrivalTime,
            //[FromQuery] int _Terminal,
            //[FromQuery] int _Gate,
            //[FromQuery] DateTime _CheckinTime,
            //[FromQuery] int _BaggageCarousel
            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var _Plane = await _context.Planes.FindAsync(_flight._Id);
                if (_Plane is not null)
                {
                // Create FlightStatus
                var flightStatus = new Status
                {
                    DepartureTime =_flight._DepartureTime,
                    ArrivalTime = _flight._ArrivalTime,
                    Terminal = _flight._Terminal,
                    Gate = _flight._Gate,
                    CheckinTime = _flight._CheckinTime,
                    BaggageCarousel = _flight._BaggageCarousel
                };

                _context.FlightStatuses.Add(flightStatus);
                await _context.SaveChangesAsync();
                    // Create Flight
                    var flight = new Flight
                    {
                        CurrentPlaneId = _Plane.Id,
                        Starting = _flight._Starting,
                        Destination = _flight._Destination,
                        Type = _flight.Type,
                        StatusId = flightStatus.Id,
                        Direction = _flight._Starting == "Wroclaw" ? "Odlot" : "Przylot",
                        Distance = _flight._Distance,
                        AvailableSeats = _Plane.Capacity
                    };

                    _context.Flights.Add(flight);
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new { Message = "Flight added successfully", FlightId = flight.Id, StatusId = flightStatus.Id });
                }
                else
                {
                    return NotFound(new { Message = "Plane not found." });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "Transaction failed", Error = ex.Message });
            }
        }

        [HttpDelete("delete-flight")]
        public async Task<IActionResult> DeleteFlight([FromQuery] int flightId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var flight = await _context.Flights.FindAsync(flightId);
                int? flightStatusId = null;
                if (flight is not null)
                {
                    flightStatusId = flight.StatusId;
                }
                if (flight == null)
                {
                    return NotFound(new { Message = "Flight not found." });
                }

                _context.Flights.Remove(flight);

                Status? flightStatus = null;
                if (flight is not null && flightStatusId is not null)
                {
                    flightStatus = await _context.FlightStatuses.FindAsync(flightStatusId);
                }

                if (flightStatus == null)
                {
                    return NotFound(new { Message = "FlightStatus not found." });
                }

                _context.FlightStatuses.Remove(flightStatus);

                // Save changes to delete the records
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();


                return Ok(new { Message = "Flight and FlightStatus deleted successfully", StatusId = flightStatusId, FlightId = flightId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "Transaction failed", Error = ex.Message });
            }
        }

        [HttpGet("arrivals")]
        public async Task<ActionResult<List<Flight>>> GetArrivals()
        {
            var arrivals = await _context.Flights
                .Where(f => f.Direction == "Przylot")
                .OrderBy(f => f.Id)
                .ToListAsync();

            return Ok(arrivals);
        }

        [HttpGet("departures")]
        public async Task<ActionResult<List<Flight>>> GetDepartures()
        {
            var arrivals = await _context.Flights
                .Where(f => f.Direction == "Odlot")
                .OrderBy(f => f.Id)
                .ToListAsync();

            return Ok(arrivals);
        }

    


    [HttpPut("updateFlight")]
        public async Task<IActionResult> UpdateFlight(
            //[FromQuery] int id,
            //[FromQuery] int _CurrentPlaneId,
            //[FromQuery] string _Starting,
            //[FromQuery] string _Destination,
            //[FromQuery] string _Type,
            //[FromQuery] int _Distance,
            //[FromQuery] DateTime _DepartureTime,
            //[FromQuery] DateTime _ArrivalTime,
            //[FromQuery] int _Terminal,
            //[FromQuery] int _Gate,
            //[FromQuery] DateTime _CheckinTime,
            //[FromQuery] int _BaggageCarousel
            [FromBody] Flight _filght
            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var _Plane = await _context.Planes.FindAsync(_filght.CurrentPlaneId);
                if (_Plane is not null)
                {
                    // Update Flight
                    var flight = await _context.Flights.FindAsync(_filght.Id);
                    if (flight == null)
                    {
                        return NotFound(new { Message = "Flight not found." });
                    }
                    {
                        flight.CurrentPlaneId = _filght.CurrentPlaneId;
                        flight.Starting = _filght.Starting;
                        flight.Destination = _filght.Destination;
                        flight.Type = _filght.Type;
                        flight.Direction = _filght.Starting == "Wroclaw" ? "Odlot" : "Przylot";
                        flight.Distance = _filght.Distance;
                        flight.AvailableSeats = _Plane.Capacity;
                    };
                    await _context.SaveChangesAsync();

                    /// Update FlightStatus
                    var flightStatus = await _context.FlightStatuses.FindAsync(flight.StatusId);
                    if (flightStatus == null)
                    {
                        return NotFound(new { Message = "Flight status not found." });
                    }
                    {
                        flightStatus.DepartureTime = _filght.Status.DepartureTime;
                        flightStatus.ArrivalTime = _filght.Status.ArrivalTime;
                        flightStatus.Terminal = _filght.Status.Terminal;
                        flightStatus.Gate = _filght.Status.Gate;
                        flightStatus.CheckinTime = _filght.Status.CheckinTime;
                        flightStatus.BaggageCarousel = _filght.Status.BaggageCarousel;
                    };

                    await _context.SaveChangesAsync();
                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new { Message = "Flight added successfully", FlightId = flight.Id, StatusId = flightStatus.Id });
                }
            else
                {
                    return NotFound(new { Message = "Plane not found." });
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction in case of error
                await transaction.RollbackAsync();

                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }


        }

    }
}
