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
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public UserController(
            AppDbContext context,
            IConfiguration configuration
            )
        {
            _configuration = configuration;
            _context = context;
        }
        //[Authorize]
        [HttpGet("GetAllTicketsByClientId")]
        public async Task<ActionResult<IEnumerable<TicketTransaction>>> GetAllTicketsByClientId([FromQuery] int ClienId)
        {
            try
            {
                var Tickets = await _context.Tickets
                    .Include(f => f.TicketTransaction)
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
        public class CreateUserRequest
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string UserName { get; set; }
            public string Phone { get; set; }
            public string Password { get; set; }
            public string Mail { get; set; }
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(

            [FromBody] CreateUserRequest request
            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string encryptedPassword = EncryptPassword(request.Password);
                if(await _context.Users.FirstOrDefaultAsync(f => f.UserName == request.UserName) != null)
                {
                    throw new Exception("istnieje już user");
                }

                 var User = new User
                {
                    Name = request.Name,
                    Surname = request.Surname,
                    UserName = request.UserName,
                    Phone = request.Phone,
                    Password = encryptedPassword,
                    Mail = request.Mail
                };

                _context.Users.Add(User);
                await _context.SaveChangesAsync();
                _context.UserRoless.Add(new UserRoles { UserRoleId = User.Id, RoleId = 2 });
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

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody]User _user)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == _user.Id);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                string encryptedPassword = EncryptPassword(_user.Password);
                user.Name = _user.Name;
                user.Surname = _user.Surname;
                user.Phone = _user.Phone;
                user.UserName = _user.UserName;
                user.Mail = _user.Mail;
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

            var user = await _context.Users.Include(f => f.UserRoles)
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
        [HttpGet("GetUserRoles")]
        public async Task<ActionResult<List<UserRoles>>> GetUserRoles([FromQuery] int userId)
        {
            try
            {
                var roles = await _context.UserRoless
                .Where(u => u.UserRoleId == userId )
                .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }



        [HttpGet("JWTToken")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> JWTToken([FromQuery] string _UserName, [FromQuery] string _Password)
        {
            string hashedInputPassword = EncryptPassword(_Password);

            var user = await _context.Users
                .Where(u => u.UserName == _UserName && u.Password == hashedInputPassword)
                .FirstOrDefaultAsync();
            if (user != null)
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
                    expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"])),
                    signingCredentials: credentials
                );
                //return Ok(new { Authorization = token });
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(user);

            }

            return Unauthorized(new { Message = "Invalid username or password." });
        }

        [HttpPost("PurchaseTicket")]
        public async Task<IActionResult> PurchaseTicket(
            
            [FromBody] Ticket _ticket
            )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var flight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == _ticket.OrderFlightId);
                if (flight == null || flight.AvailableSeats <= 0)
                {
                    return BadRequest(new { Message = "Flight not found or no available seats." });
                }

                flight.AvailableSeats -= 1;
                _context.Flights.Update(flight);
                await _context.SaveChangesAsync();

                var ticketTransaction = new TicketTransaction
                {
                    Amount = _ticket.TicketTransaction.Amount,
                    TransactionUserId = _ticket.UserId,
                    CardDetail = _ticket.TicketTransaction.CardDetail,
                    Status = "Completed",
                    Date = DateTime.Now
                };
                _context.TicketTransactions.Add(ticketTransaction);
                await _context.SaveChangesAsync();

                var transactionId = ticketTransaction.Id;

                var ticket = new Ticket
                {
                    UserId = _ticket.UserId,
                    OrderFlightId = _ticket.OrderFlightId,
                    OrderDate = DateTime.Now,
                    Insurence = _ticket.Insurence,
                    TransactionId = transactionId,
                    SeatingType = _ticket.SeatingType,
                    SeatingNumber = _ticket.SeatingType
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

        [HttpPost("AddRoleToUser")]
        public async Task<IActionResult> AddRoleToUser([FromBody] UserRoles _userRoles)
        {
            var user = await _context.Users.FirstOrDefaultAsync(f => f.Id == _userRoles.UserRoleId);
            if (user != null)
            {

            var role = new UserRoles
            {
                UserRoleId = _userRoles.UserRoleId,
                RoleId = _userRoles.RoleId
            };
  
                _context.UserRoless.Add(role);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Authentication successful.",
                });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }
        }

        [HttpGet("AllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> AllUsers()
        {
            try
            {
                var users = await _context.Users
                    .OrderBy(f => f.Id)
                    .ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving data.", Details = ex.Message });
            }
        }
    }
}
