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
using System.Collections.Generic;
using System.Security.Cryptography;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LotniskoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        public UserController(AppDbContext context, IConfiguration configuration, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("GetAllTicketsByClientId")]
        public async Task<ActionResult<IEnumerable<TicketTransaction>>> GetAllTicketsByClientId([FromQuery] int ClienId)
        {
            try
            {
                var Tickets = await _context.Tickets
                    .Where(tt => tt.UserId == ClienId)
                    .ToListAsync();
                return Ok(Tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }

        [HttpGet("TransactionDetails")]
        public async Task<ActionResult<TicketTransaction>> TransactionDetails([FromQuery] int _TransactionId)
        {
            try
            {
                var transaction = await _context.TicketTransactions
                .FirstOrDefaultAsync(f => f.Id == _TransactionId);

                if (transaction == null)
                {
                    return NotFound();
                }
                return Ok(transaction);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(
            [FromQuery] string _name,
            [FromQuery] string _surname,
            [FromQuery] string _username,
            [FromQuery] string _phone,
            [FromQuery] string _password,
            [FromQuery] string _mail

            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string encryptedPassword = EncryptPassword(_password);

                var User = new User
                {
                    Name = _name,
                    Surname = _surname,
                    UserName = _username,
                    Phone = _phone,
                    Password = encryptedPassword,
                    Mail = _mail
                };

                _context.Users.Add(User);
                await _context.SaveChangesAsync();


                await transaction.CommitAsync();

                return Ok(new { Message = "User created successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message/*, InnerException = ex.InnerException.Message*/});
            }
        }

        private string EncryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString(); // Return the encrypted password as a hexadecimal string
            }
        }

        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromQuery] string _password, [FromQuery] int _userId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == _userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                string encryptedPassword = EncryptPassword(_password);

                user.Password = encryptedPassword;
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromQuery] string _UserName, [FromQuery] string _Password)
        {
            string hashedInputPassword = EncryptPassword(_Password);

            var user = await _context.Users
                .Where(u => u.UserName == _UserName && u.Password == hashedInputPassword)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                return Ok(new
                {
                    Message = "Authentication successful.",
                    UserId = user.Id,
                    UserName = user.UserName
                });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }
        }

        [HttpPost("JWTToken")]
        [AllowAnonymous]
        public async Task<IActionResult> JWTToken([FromQuery] string _UserName, [FromQuery] string _Password)
        {
            var auth = await Authenticate(_UserName, _Password);
            if (auth is OkObjectResult)
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                new Claim(ClaimTypes.Name, _UserName),
                new Claim(ClaimTypes.Role, "Admin") // Przykładowa rola
                };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"])),
                    signingCredentials: credentials
                );
                //return Ok(new { Authorization = token });
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new
                {
                    token = tokenString,
                    tokenType = "Bearer",
                    expiresIn = int.Parse(jwtSettings["ExpiresInMinutes"]) * 60
                });

            }

            return Unauthorized(new { Message = "Invalid username or password.",auth = auth });
        }

        [HttpPost("PurchaseTicket")]
        public async Task<IActionResult> PurchaseTicket(
            [FromQuery] int _OrderFlightId,
            [FromQuery] int _Amount,
            [FromQuery] bool _Insurance,
            [FromQuery] int _SeatingType,
            [FromQuery] string _CardDetail,
            [FromQuery] int SeatingNumber,
            [FromQuery] int _ClientId
            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var flight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == _OrderFlightId);
                if (flight == null || flight.AvailableSeats <= 0)
                {
                    return BadRequest(new { Message = "Flight not found or no available seats." });
                }

                flight.AvailableSeats -= 1;
                _context.Flights.Update(flight);
                await _context.SaveChangesAsync();

                var ticketTransaction = new TicketTransaction
                {
                    Amount = _Amount,
                    TransactionUserId = _ClientId,
                    CardDetail = _CardDetail,
                    Status = "Completed",
                    Date = DateTime.Now
                };
                _context.TicketTransactions.Add(ticketTransaction);
                await _context.SaveChangesAsync();

                var transactionId = ticketTransaction.Id;

                var ticket = new Ticket
                {
                    UserId = _ClientId,
                    OrderFlightId = _OrderFlightId,
                    OrderDate = DateTime.Now,
                    Insurence = _Insurance,
                    TransactionId = transactionId,
                    SeatingType = _SeatingType,
                    SeatingNumber = _SeatingType
                };
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { Message = "Ticket purchased successfully.",ticketId = ticket.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message});
            }
        }
    }
}
