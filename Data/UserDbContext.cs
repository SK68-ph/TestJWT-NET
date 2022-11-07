using JwtApp.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace TestJWT.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
               : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<NoteModel> Notes { get; set; }
    }
}
