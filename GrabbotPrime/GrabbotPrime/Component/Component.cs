using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GrabbotPrime.Component
{
    public static class ComponentRegistry
    {
        public static Type GetComponentTypeFromName(string name)
        {
            switch (name)
            {
                case DiscordBot.ComponentTypeName:
                    return typeof(DiscordBot);
                default:
                    throw new ArgumentException($"'{name}' is not a valid component.", nameof(name));
            }
        }
    }

    class ComponentBase
    {
        public const string ComponentTypeName = "generic";

        private readonly object _readWriteLock = new object();

        private readonly string _uuid;

        private readonly IMongoCollection<BsonDocument> _collection;

        private BsonDocument InternalDocument
        {
            get
            {
                var doc = _collection
                    .FindAsync(Builders<BsonDocument>.Filter.Eq("uuid", _uuid)).Result
                    .FirstOrDefault();

                if (doc == null)
                {
                    doc = new BsonDocument { { "uuid", _uuid }, { "type", MyComponentTypeName } };
                    _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("uuid", _uuid), doc, new ReplaceOptions { IsUpsert = true }).Wait();
                }

                return doc;
            }
        }

        public ComponentBase(IMongoCollection<BsonDocument> collection, string uuid = null)
        {
            _collection = collection.WithWriteConcern(WriteConcern.Acknowledged);
            _uuid = uuid ?? Guid.NewGuid().ToString();
        }

        public Core Core { get; set; }

        protected virtual string MyComponentTypeName => ComponentTypeName;

        public virtual void Init()
        {
        }

        public virtual void Tick()
        {
        }

        public virtual void TickRare()
        {

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

                _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("uuid", _uuid), doc, new ReplaceOptions { IsUpsert = true }).Wait();
            }
        }
    }
}
