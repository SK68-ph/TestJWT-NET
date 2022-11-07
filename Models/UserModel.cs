using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JwtApp.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public byte[] Salt { get; set; }


    }

    public class NoteModel
    {
        public int Id { get; set; }
        public string NoteTitle { get; set; }
        public string NoteDescription { get; set; }
        public int UserModelId { get; set; }
        public UserModel UserModel { get; set; }

    }

}
