using Driscod.Tracking.Objects;
using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Command.Context;
using GrabbotPrime.Component.SongQueue;
using System;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Discord.Components
{
    public class DiscordCommandContext : ICommandContext
    {
        private Channel _channel;

        private User _user;

        private TimeSpan _timeout;

        public DiscordCommandContext(Channel channel, User user, TimeSpan timeout)
        {
            _channel = channel;
            _user = user;
            _timeout = timeout;
        }

        public ISingleSongPlayer GetSongPlayerForSource(IAudioStreamSource source)
        {
            return new DiscordAudioPlayer(_channel, _user, source);
        }

        public Task SendMessage(string message)
        {
            _channel.SendMessage(message);
            return Task.CompletedTask;
        }

        public async Task<string> WaitForMessage()
        {
            var tcs = new TaskCompletionSource<Message>();

            EventHandler<Message> handler = (_, message) =>
            {
                if (message.Author != _channel.Bot.User)
                {
                    tcs.SetResult(message);
                }
            };

            _channel.Bot.OnMessage += handler;

            try
            {
                await Task.WhenAny(tcs.Task, Task.Delay(_timeout));

                if (!tcs.Task.IsCompleted)
                {
                    _channel.SendMessage("Timed out.");
                    throw new TimeoutException();
                }

                return tcs.Task.Result.Content;
            }
            finally
            {
                _channel.Bot.OnMessage -= handler;
            }
        }
    }
}
