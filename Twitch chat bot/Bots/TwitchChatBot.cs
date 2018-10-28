using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Api;
using Twitch_chat_bot.Resources;
using TwitchLib.Api.V5.Models.Users;

namespace Twitch_chat_bot.Core
{
    public class TwitchChatBot
    {
        #region Ctor

        public TwitchChatBot()
        {
            Api = new TwitchAPI();
            mContext = new RouletteDbContext();
            mContext.Database.EnsureCreated();
            Connect();
        }

        #endregion

        #region Private properties
        /// <summary>
        /// Credentials needed for API calls
        /// </summary>
        private ConnectionCredentials credentials => new ConnectionCredentials(Keys.UserName, Keys.Token);

        /// <summary>
        /// Abstraction for receiving and sending messages
        /// </summary>
        private TwitchClient twitchClient { get; set; }

        /// <summary>
        /// Api abstraction for making additional calls
        /// </summary>
        private TwitchAPI Api { get; set; }

        /// <summary>
        /// Db context for roulette game
        /// </summary>
        private RouletteDbContext mContext { get; set; }

        #endregion

        #region Service methods

        internal void Connect()
        {
            Console.WriteLine("Connecting");

            Api.Settings.ClientId = Keys.ClientId;

            twitchClient = new TwitchClient();

            twitchClient.Initialize(credentials, Keys.ChannelName);

            twitchClient.OnMessageThrottled += TwitchClient_OnMessageThrottled;

            twitchClient.OnLog += TwitchClient_OnLog;

            twitchClient.OnConnectionError += TwitchClient_OnConnectionError;

            twitchClient.OnMessageReceived += OnMessageReceived;

            twitchClient.Connect();
        }

        private void TwitchClient_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error happened!: {e.Error}");
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private async void TwitchClient_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        #endregion

        #region Listeners


        private void TwitchClient_OnMessageThrottled(object sender, TwitchLib.Communication.Events.OnMessageThrottledEventArgs e)
        {
            Console.WriteLine("Message was not sent because it was throttled");
        }

        internal void DisConnect()
        {
            Console.WriteLine("Disconnecting");

            twitchClient.Disconnect();
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {

            #region Info logic

            if (e.ChatMessage.Message.StartsWith("!info", StringComparison.InvariantCultureIgnoreCase))
            {
                twitchClient.SendMessage(Keys.ChannelName, "Commands in chat: !info !roulette !roulette x !roulette refresh");
                return;
            }
           
            #endregion

            #region Uptime logic

            if (e.ChatMessage.Message.StartsWith("!uptime", StringComparison.InvariantCultureIgnoreCase))
            {
                twitchClient.SendMessage(Keys.ChannelName, "User was online for : :" + GetUpTime()?.ToString("h:mm:ss") ?? "Offline");
                return;
            }

            #endregion

            #region Showing balance in roulette logic

            if (e.ChatMessage.Message.Equals("!roulette", StringComparison.InvariantCultureIgnoreCase))
            {

                var user = (TwitchUser)mContext.Find(typeof(TwitchUser), e.ChatMessage.UserId) ?? null;

                if(user == null)
                {
                    user = new TwitchUser();
                    user.Balance += 100;
                    user.CanPlay = true;
                    user.TwitchId = e.ChatMessage.UserId;
                    mContext.Add(user);

                    mContext.SaveChanges();
                }

                twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username + 
                " Your current balance is :" +
                user.Balance);

                return;
            }


            #endregion

            #region Playing in roulette game

            if (e.ChatMessage.Message.StartsWith("!roulette")
               && long.TryParse(e.ChatMessage.Message.Substring("!roulette".Length + 1), out long bid))
            {
                var user = (TwitchUser)mContext.Find(typeof(TwitchUser), e.ChatMessage.UserId) ?? null;

                if (user == null)
                {
                    user = new TwitchUser();
                    user.Balance += 100;
                    user.CanPlay = true;
                    user.TwitchId = e.ChatMessage.UserId;
                    mContext.Add(user);

                    mContext.SaveChanges();
                }

                if (user.Balance < bid)
                {
                    twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username + 
                    " Can't bid that much: your balance is only : " +
                    user.Balance);

                    return;
                }

                var entirelyRandomNumber = new Random().Next(0, 2);

                if (entirelyRandomNumber == 1)
                {
                    user.Balance += bid;

                    twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username 
                        + " You won roflanEbashu! Your balance is now : " +
                    user.Balance);

                    return;
                }

                user.Balance = (user.Balance - bid < 0) ? 0 : user.Balance - bid;

                twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username + 
                    " You lost roflanEbalo! Your balance is now : " +
                user.Balance);

                mContext.SaveChanges();
            }

            #endregion

            #region Refreshing user's balance

            if (e.ChatMessage.Message.StartsWith("!roulette") &&
              (e.ChatMessage.Message.Substring("!roulette".Length + 1) == "refresh"))
            {
                var user = (TwitchUser)mContext.Find(typeof(TwitchUser), e.ChatMessage.UserId) ?? null;

                if (user == null)
                {
                    twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username 
                        + " Can't refresh your balance, try losing all your balance TriHard "
                    );

                    return;
                }

                if(user.Balance == 0)
                {
                    twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username
                          + " Refreshed your balance. Good luck :)"
                      );

                    user.Balance += 100;
                }

                twitchClient.SendMessage(Keys.ChannelName, "@" + e.ChatMessage.Username
                        + " Can't refresh your balance, try losing all your balance TriHard "
                    );
            }

            #endregion
        }

        #endregion

        #region Helper methods

        private TimeSpan? GetUpTime()
        {
            var userid = GetUserByID(Keys.UserName);

            if(userid == null)
            {
                return null;
            }

            return Api.V5.Streams.GetUptimeAsync(userid).Result;
        }

        private string GetUserByID(string Username)
        {
            User[] userList = Api.V5.Users.GetUserByNameAsync(Username).Result.Matches;

            if (userList == null || userList.Length == 0)
                return null;

            return userList[0].Id;
        }

        #endregion

    }
}
