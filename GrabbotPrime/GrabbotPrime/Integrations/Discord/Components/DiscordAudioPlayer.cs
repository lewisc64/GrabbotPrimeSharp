using Driscod.Audio;
using Driscod.Tracking.Objects;
using GrabbotPrime.Commands.Audio.Source;
using GrabbotPrime.Component.SongQueue;
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

        private IAudioStreamSource _source;

        public DiscordAudioPlayer(Channel channel, User user, IAudioStreamSource source)
        {
            _channel = channel;
            _user = user;
            _source = source;
        }

        public async Task Play(CancellationToken cancellationToken, Action donePlayingCallback = null)
        {
            try
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
                    await connection.PlayAudio(new AudioFile(_source.StreamUrl), cancellationToken: cancellationToken);
                }
            }
            finally
            {
                donePlayingCallback?.Invoke();
            }
        }
    }
}
