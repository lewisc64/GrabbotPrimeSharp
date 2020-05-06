using Driscod.DiscordObjects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driscod
{
    public class Bot
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<Type, Dictionary<string, DiscordObject>> Objects = new Dictionary<Type, Dictionary<string, DiscordObject>>();

        private List<Shard> _shards;

        public IEnumerable<Emoji> Emojis => GetObjects<Emoji>();

        public IEnumerable<Guild> Guilds => GetObjects<Guild>();

        public Bot(string token)
        {
            CreateShards(token);
            CreateDispatchListeners();
        }

        public void Start()
        {
            foreach (var shard in _shards)
            {
                shard.Start();
            }
            while (!_shards.All(x => x.Ready)) { }
        }

        internal IEnumerable<T> GetObjects<T>()
            where T : DiscordObject
        {
            return Objects.ContainsKey(typeof(T)) ? Objects[typeof(T)].Values.Cast<T>() : new T[0];
        }

        internal T GetObject<T>(string id)
            where T : DiscordObject
        {
            if (Objects.ContainsKey(typeof(T)) && Objects[typeof(T)].ContainsKey(id))
            {
                return (T)Objects[typeof(T)][id];
            }
            return null;
        }

        internal void CreateOrUpdateObject<T>(BsonDocument doc)
            where T : DiscordObject, new()
        {
            var type = typeof(T);

            if (!Objects.ContainsKey(type))
            {
                Objects[type] = new Dictionary<string, DiscordObject>();
            }

            var table = Objects[type];
            var id = doc["id"].AsString;

            if (!table.ContainsKey(id))
            {
                table[id] = new T();
                table[id].Bot = this;
            }

            table[id].UpdateFromDocument(doc);
        }

        private void CreateShards(string token)
        {
            _shards = new List<Shard>
            {
                new Shard(token, 0, 1),
            };
        }

        private void CreateDispatchListeners()
        {
            foreach (var shard in _shards)
            {
                shard.AddListener(
                    MessageType.Dispatch,
                    data =>
                    {
                        CreateOrUpdateObject<Guild>(data);
                    },
                    eventName: "GUILD_CREATE");

                shard.AddListener(
                    MessageType.Dispatch,
                    data =>
                    {
                        CreateOrUpdateObject<Guild>(data);
                    },
                    eventName: "GUILD_UPDATE");

                shard.AddListener(
                    MessageType.Dispatch,
                    data =>
                    {
                        data["id"] = data["guild_id"];
                        CreateOrUpdateObject<Guild>(data);
                    },
                    eventName: "GUILD_EMOJIS_UPDATE");

                shard.AddListener(
                    MessageType.Dispatch,
                    data =>
                    {
                        GetObject<Guild>(data["guild_id"].AsString)?.UpdatePresence(data);
                    },
                    eventName: "PRESENCE_UPDATE");
            }
        }
    }
}
