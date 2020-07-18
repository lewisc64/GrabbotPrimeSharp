using GrabbotPrime.Component;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;
using System;
using System.Threading;
using GrabbotPrime.Commands;

namespace GrabbotPrime
{
    public class Core
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string DatabaseName = "grabbotPrime";

        private const string ComponentsCollectionName = "components";

        private readonly IMongoClient _mongoClient;

#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        private Thread _tickThread;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables

        public Core(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;

            LoadComponents();
        }

        public bool Running { get; private set; } = false;

        private List<IComponent> Components { get; set; } = new List<IComponent>();

        private IEnumerable<ICommand> Commands { get; set; } = CommandsRegistry.GetCommands();

        private IMongoDatabase Database => _mongoClient.GetDatabase(DatabaseName);

        private IMongoCollection<BsonDocument> RemoteComponentsCollection => Database.GetCollection<BsonDocument>(ComponentsCollectionName);

        public void Start()
        {
            if (Running)
            {
                throw new InvalidOperationException("Core is already running.");
            }
            Logger.Info("Starting core...");

            foreach (var component in Components)
            {
                component.Init();
            }

            foreach (var command in Commands)
            {
                command.Core = this;
            }

            Running = true;
            _tickThread = new Thread(TickLoop);
            _tickThread.Start();
        }

        public void TickLoop()
        {
            while (Running)
            {
                foreach (var component in Components)
                {
                    component.Tick();
                }
                Thread.Sleep(20);
            }
        }

        public T CreateComponent<T>(string uuid = null)
            where T : IComponent
        {
            if (uuid == null)
            {
                Components.Add((T)Activator.CreateInstance(typeof(T), new object[] { RemoteComponentsCollection, null }));
            }
            else
            {
                Components.Add((T)Activator.CreateInstance(typeof(T), new object[] { RemoteComponentsCollection, uuid }));
            }
            Components.Last().Core = this;
            if (Running)
            {
                Components.Last().Init();
            }

            var component = (T)Components.Last();

            Logger.Info($"Created component '{component}'.");

            return component;
        }

        public T CreateComponentIfNotExists<T>(string uuid = null)
            where T : IComponent
        {
            if (uuid == null)
            {
                var component = Components.FirstOrDefault(x => x.GetType() == typeof(T));
                if (component != null)
                {
                    return (T)component;
                }
            }
            else
            {
                var component = Components.FirstOrDefault(x => x.Uuid == uuid);
                if (component != null)
                {
                    return (T)component;
                }
            }

            return CreateComponent<T>(uuid);
        }

        public IEnumerable<T> GetComponents<T>()
            where T : IComponent
        {
            return Components.Where(x => x is T).Cast<T>();
        }

        public void LoadComponents()
        {
            Logger.Info("Loading components from database...");
            lock (Components)
            {
                Components.Clear();
                foreach (var document in RemoteComponentsCollection.FindAsync(Builders<BsonDocument>.Filter.Empty).Result.ToEnumerable())
                {
                    Type componentType = ComponentRegistry.GetComponentTypeFromName(document["type"].AsString);

                    if (!document.Contains("uuid"))
                    {
                        document["uuid"] = Guid.NewGuid().ToString();
                        RemoteComponentsCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", document["_id"]), document, new ReplaceOptions { IsUpsert = false });
                    }

                    Components.Add((ComponentBase)Activator.CreateInstance(componentType, new object[] { RemoteComponentsCollection, document["uuid"].AsString }));
                    var component = Components.Last();

                    component.Core = this;

                    Logger.Info($"Found component '{component}'.");

                    if (Running)
                    {
                        component.Init();
                    }
                }
            }
        }

        public ICommand RecogniseCommand(string command)
        {
            foreach (var commandInstance in Commands)
            {
                if (commandInstance.Recognise(command))
                {
                    return commandInstance;
                }
            }
            throw new ArgumentException("Command does not have a match.", nameof(command));
        }
    }
}
