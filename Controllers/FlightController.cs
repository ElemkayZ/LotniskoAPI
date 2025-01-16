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
                .FirstOrDefaultAsync(f => f.Id == id);

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

        [HttpPost("add-flight")]
        public async Task<IActionResult> AddFlight(
            [FromQuery] int _CurrentPlaneId,
            [FromQuery] string _Starting,
            [FromQuery] string _Destination,
            [FromQuery] string _Type,
            [FromQuery] int _Distance,
            [FromQuery] DateTime _DepartureTime,
            [FromQuery] DateTime _ArrivalTime,
            [FromQuery] int _Terminal,
            [FromQuery] int _Gate,
            [FromQuery] DateTime _CheckinTime,
            [FromQuery] int _BaggageCarousel
            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var _Plane = await _context.Planes.FindAsync(_CurrentPlaneId);
                if (_Plane is not null)
                {
                // Create FlightStatus
                var flightStatus = new Status
                {
                    DepartureTime = _DepartureTime,
                    ArrivalTime = _ArrivalTime,
                    Terminal = _Terminal,
                    Gate = _Gate,
                    CheckinTime = _CheckinTime,
                    BaggageCarousel = _BaggageCarousel
                };

                _context.FlightStatuses.Add(flightStatus);
                await _context.SaveChangesAsync();

                    // Create Flight
                    var flight = new Flight
                    {
                        CurrentPlaneId = _CurrentPlaneId,
                        Starting = _Starting,
                        Destination = _Destination,
                        Type = _Type,
                        StatusId = flightStatus.Id,
                        Direction = _Starting == "Wroclaw" ? "Odlot" : "Przylot",
                        Distance = _Distance,
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
            [FromQuery] int id,
            [FromQuery] int _CurrentPlaneId,
            [FromQuery] string _Starting,
            [FromQuery] string _Destination,
            [FromQuery] string _Type,
            [FromQuery] int _Distance,
            [FromQuery] DateTime _DepartureTime,
            [FromQuery] DateTime _ArrivalTime,
            [FromQuery] int _Terminal,
            [FromQuery] int _Gate,
            [FromQuery] DateTime _CheckinTime,
            [FromQuery] int _BaggageCarousel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var _Plane = await _context.Planes.FindAsync(_CurrentPlaneId);
                if (_Plane is not null)
                {
                    // Update Flight
                    var flight = await _context.Flights.FindAsync(id);
                    if (flight == null)
                    {
                        return NotFound(new { Message = "Flight not found." });
                    }
                    {
                        flight.CurrentPlaneId = _CurrentPlaneId;
                        flight.Starting = _Starting;
                        flight.Destination = _Destination;
                        flight.Type = _Type;
                        flight.Direction = _Starting == "Wroclaw" ? "Odlot" : "Przylot";
                        flight.Distance = _Distance;
                        flight.AvailableSeats = 69;
                    };
                    await _context.SaveChangesAsync();

                    /// Update FlightStatus
                    var flightStatus = await _context.FlightStatuses.FindAsync(flight.StatusId);
                    if (flightStatus == null)
                    {
                        return NotFound(new { Message = "Flight status not found." });
                    }
                    {
                        flightStatus.DepartureTime = _DepartureTime;
                        flightStatus.ArrivalTime = _ArrivalTime;
                        flightStatus.Terminal = _Terminal;
                        flightStatus.Gate = _Gate;
                        flightStatus.CheckinTime = _CheckinTime;
                        flightStatus.BaggageCarousel = _BaggageCarousel;
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
