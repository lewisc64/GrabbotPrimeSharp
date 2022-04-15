using Driscod.Audio;
using Driscod.Tracking.Objects;
using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Integrations.Base.Components;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Discord.Components
{
    public class DiscordAudioPlayer : ISingleSongPlayer
    {
        private Channel _channel;

        private User _user;

        public IAudioStreamSource Source { get; private set; }

        public DiscordAudioPlayer(Channel channel, User user, IAudioStreamSource source)
        {
            _channel = channel;
            _user = user;

            Source = source;
        }

        public async Task Play(CancellationToken cancellationToken)
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
                await connection.PlayAudio(new AudioFile(Source.StreamUrl), cancellationToken: cancellationToken);
            }
        }
    }
}
