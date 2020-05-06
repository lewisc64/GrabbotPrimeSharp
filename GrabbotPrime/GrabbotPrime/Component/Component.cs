using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GrabbotPrime.Component
{
    class ComponentBase
    {
        private object _readWriteLock = new object();

        private string _uuid;

        private readonly IMongoCollection<BsonDocument> _collection;

        public string Test
        {
            get
            {
                return GetPropertyByName("test")?.AsString;
            }
            set
            {
                SetPropertyByName("test", value);
            }
        }

        protected virtual string ComponentTypeName => "generic";

        private BsonDocument InternalDocument
        {
            get
            {
                return _collection
                    .FindAsync(Builders<BsonDocument>.Filter.Eq("uuid", _uuid)).Result
                    .FirstOrDefault() ?? new BsonDocument { { "uuid", _uuid }, { "type", ComponentTypeName } };
            }
        }

        public ComponentBase(IMongoCollection<BsonDocument> collection, string uuid = null)
        {
            _collection = collection.WithWriteConcern(WriteConcern.Acknowledged);
            _uuid = uuid ?? Guid.NewGuid().ToString();
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
