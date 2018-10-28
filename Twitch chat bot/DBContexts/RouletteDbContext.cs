using Microsoft.EntityFrameworkCore;

namespace Twitch_chat_bot.Core
{
    public class RouletteDbContext : DbContext
    {
        /// <summary>
        /// Users participating in roulette
        /// </summary>
        public DbSet<TwitchUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Dennibot.Roullete.db");
        }
    }
}
