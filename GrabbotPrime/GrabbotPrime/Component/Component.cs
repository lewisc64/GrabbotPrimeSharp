using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Device;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GrabbotPrime.Component
{
    public static class ComponentRegistry
    {
        public static Type GetComponentTypeFromName(string name)
        {
            return GetComponentTypes()
                .FirstOrDefault(x => x.Name == name) ?? throw new ArgumentException($"'{name}' is not a valid component.", nameof(name));
        }

        public static IEnumerable<Type> GetComponentTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(IComponent).IsAssignableFrom(x));
        }
    }

    public interface IComponent
    {
        Core Core { get; set; }

        ObjectId Id { get; }

        Task Start();

        Task Stop();

        Task Tick();

        Task TickRare();
    }

    public interface IHasDevices : IComponent
    {
        IEnumerable<IDevice> GetDevices();
    }

    public interface IHasOutputCapability : IComponent
    {
    }

    public interface IIsService : IComponent
    {
    }

    public interface IIsAudioSearchService : IIsService
    {
        int? Priority { get; set; }

        string ServiceIdentifier { get; set; }

        IAsyncEnumerable<IAudioStreamSource> SearchForSong(string query);

        IAsyncEnumerable<IAudioStreamSource> SearchForSongs(string query);
    }

    public interface IIsImageSearchService : IIsService
    {
        int? Priority { get; set; }

        string ServiceIdentifier { get; set; }

        IEnumerable<string> SearchForRandomImageUrls(string query);
    }

    public abstract class ComponentBase : IComponent
    {
        private readonly object _readWriteLock = new object();

        private readonly IMongoCollection<BsonDocument> _collection;

        private BsonDocument InternalDocument
        {
            get
            {
                var doc = _collection
                    .FindAsync(Builders<BsonDocument>.Filter.Eq("_id", Id)).Result
                    .First();

                return doc;
            }
        }

        protected ComponentBase(IMongoCollection<BsonDocument> collection, ObjectId? id = null)
        {
            _collection = collection.WithWriteConcern(WriteConcern.Acknowledged);
            Id = id ?? ObjectId.GenerateNewId();
            if (id == null)
            {
                PokeDatabase();
            }
        }

        public Core Core { get; set; }

        public ObjectId Id { get; }

        public virtual Task Start()
        {
            // Method intentionally left empty.
            return Task.CompletedTask;
        }

        public virtual Task Stop()
        {
            // Method intentionally left empty.
            return Task.CompletedTask;
        }

        public virtual Task Tick()
        {
            // Method intentionally left empty.
            return Task.CompletedTask;
        }

        public virtual Task TickRare()
        {
            // Method intentionally left empty.
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Id}";
        }

        protected BsonValue GetPropertyByName(string name)
        {
            lock (_readWriteLock)
            {
                InternalDocument.TryGetValue(name, out BsonValue value);
                return value;
            }
        }

        protected void SetPropertyByName(string name, BsonValue value)
        {
            lock (_readWriteLock)
            {
                var doc = InternalDocument;
                doc[name] = value;

                _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("_id", Id), doc, new ReplaceOptions { IsUpsert = true }).Wait();
            }
        }

        private void PokeDatabase()
        {
            var doc = new BsonDocument { { "_id", Id }, { "type", GetType().Name } };
            _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("_id", Id), doc, new ReplaceOptions { IsUpsert = true }).Wait();
        }
    }
}
