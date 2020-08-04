using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GrabbotPrime.Device;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GrabbotPrime.Component
{
    public static class ComponentRegistry
    {
        public static Type GetComponentTypeFromName(string name)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(IComponent).IsAssignableFrom(x))
                .FirstOrDefault(x => x.Name == name) ?? throw new ArgumentException($"'{name}' is not a valid component.", nameof(name));
        }
    }

    public interface IComponent
    {
        Core Core { get; set; }

        string Uuid { get; }

        void Init();

        void Tick();

        void TickRare();
    }

    public interface IHasDevices : IComponent
    {
        IEnumerable<IDevice> GetDevices();
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
                    .FindAsync(Builders<BsonDocument>.Filter.Eq("uuid", Uuid)).Result
                    .FirstOrDefault();

                if (doc == null)
                {
                    doc = new BsonDocument { { "uuid", Uuid }, { "type", GetType().Name } };
                    _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("uuid", Uuid), doc, new ReplaceOptions { IsUpsert = true }).Wait();
                }

                return doc;
            }
        }

        protected ComponentBase(IMongoCollection<BsonDocument> collection, string uuid = null)
        {
            _collection = collection.WithWriteConcern(WriteConcern.Acknowledged);
            Uuid = uuid ?? Guid.NewGuid().ToString();
        }

        public Core Core { get; set; }

        public string Uuid { get; }

        public virtual void Init()
        {
            // Method intentionally left empty.
        }

        public virtual void Tick()
        {
            // Method intentionally left empty.
        }

        public virtual void TickRare()
        {
            // Method intentionally left empty.
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Uuid}";
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

                _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("uuid", Uuid), doc, new ReplaceOptions { IsUpsert = true }).Wait();
            }
        }
    }
}
