using Driscod.Audio;
using Driscod.Tracking.Objects;
using GrabbotPrime.Commands.Audio.Source;
using GrabbotPrime.Commands.Context;
using System;
using System.Linq;
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

        public async Task<bool> PlayAudio(IAudioStreamSource source)
        {
            if (_channel.IsDm)
            {
                throw new NotSupportedException();
            }

            var voiceChannel = _channel.Guild.VoiceStates.FirstOrDefault(x => x.User == _user)?.Channel;

            if (voiceChannel == null)
            {
                throw new NotSupportedException();
            }

            voiceChannel.Guild.VoiceConnection?.Disconnect();

            using (var connection = voiceChannel.ConnectVoice())
            {
                await connection.PlayAudio(new AudioFile(source.StreamUrl));
            }

            return true;
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
