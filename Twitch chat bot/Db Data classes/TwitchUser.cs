using System.ComponentModel.DataAnnotations;

namespace Twitch_chat_bot.Core
{
    public class TwitchUser
    {
        [Key]
        /// <summary>
        /// Id of a user on twitch
        /// </summary>
        public string TwitchId { get; set; }

        /// <summary>
        /// Balance of a user
        /// </summary>
        public long Balance { get; set; }

        /// <summary>
        /// Indicates if this user is banned or notw
        /// </summary>
        public bool CanPlay { get; set; }
    }
}
