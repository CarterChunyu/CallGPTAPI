using GptApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GptApi.Data
{
    public class GptContext:DbContext
    {
        public GptContext(DbContextOptions<GptContext> option):base(option)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
