using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioAPI.Data;
using PortfolioAPI.DTOs;
using PortfolioAPI.Models;

namespace PortfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ApplicationDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ContactResponseDto>> CreateContact([FromBody] ContactDto contactDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ContactResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", errors)
                    });
                }

                var contact = new Contact
                {
                    Name = contactDto.Name,
                    Email = contactDto.Email,
                    Subject = contactDto.Subject,
                    Message = contactDto.Message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New contact form submitted by {contact.Name} ({contact.Email})");

                return Ok(new ContactResponseDto
                {
                    Success = true,
                    Message = "Thank you for your message! I'll get back to you soon.",
                    ContactId = contact.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact");
                return StatusCode(500, new ContactResponseDto
                {
                    Success = false,
                    Message = "Something went wrong. Please try again later."
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            try
            {
                var contacts = await _context.Contacts
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contacts");
                return StatusCode(500, "Error retrieving contacts");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(int id)
        {
            try
            {
                var contact = await _context.Contacts.FindAsync(id);

                if (contact == null)
                {
                    return NotFound();
                }

                return Ok(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contact with ID {id}");
                return StatusCode(500, "Error retrieving contact");
            }
        }

        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var contact = await _context.Contacts.FindAsync(id);
                if (contact == null)
                {
                    return NotFound();
                }

                contact.IsRead = true;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking contact {id} as read");
                return StatusCode(500, "Error updating contact");
            }
        }
    }
}
