using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using GrabbotPrime.Component;
using Driscod;

namespace GrabbotPrime
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://localhost");
            var database = client.GetDatabase("grabbotprime");
            var componentsCollection = database.GetCollection<BsonDocument>("components");

            var shard = new Shard("[REDACTED]", 0, 1);
            shard.Start();

            Console.ReadKey();
        }
    }
}
