using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driscod
{
    public static class BsonDocumentExtensions
    {
        public static BsonValue GetValueOrNull(this BsonDocument doc, string key)
        {
            return !doc.Contains(key) | doc[key].IsBsonNull ? null : doc[key];
        }
    }
}
