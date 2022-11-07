using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using TestJWT.Data;
using TestJWT.Models;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {

        private readonly UserDbContext _context;

        public NotesController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [Authorize]
        public IActionResult GetNotes()
        {
            var currentUser = GetCurrentUser();
            try
            {
                var curUser = _context.Users.First(o => o.Username == currentUser.Username);
                var Note = (from user in _context.Users
                             join note in _context.Notes
                             on curUser.Id equals note.UserModelId
                             select new
                             {
                                 NoteItem = note.NoteTitle,
                                 NoteDescription = note.NoteDescription
                             });
                return Ok(Note.ToJson());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.HelpLink);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<NoteModel>> PostNoteModel(NoteModel noteModel)
        {
            _context.Notes.Add(noteModel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("")]
        [Authorize]
        public IActionResult ModifyNote([FromBody] NoteItem noteData)
        {
            var currentUser = GetCurrentUser();
            try
            {
                //var curUser = _context.Users.First(o => o.Username == currentUser.Username);
                //var test = _context.Notes.First(o => o.Id == curUser.Id);
                return Ok(noteData.NoteTitle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.HelpLink);
            }
        }

        private UserModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new UserModel
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value
                };
            }
            return null;
        }
    }
}
