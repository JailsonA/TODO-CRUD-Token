using Ju.Model;
using Microsoft.EntityFrameworkCore;

namespace Ju.Data
{
    public class TODODbContext : DbContext
    {
        public TODODbContext(DbContextOptions<TODODbContext> options) : base(options)
        {
        }

        public DbSet<TodoModel> todos { get; set; }
        public DbSet<UserModel> users { get; set; }
    }
}
