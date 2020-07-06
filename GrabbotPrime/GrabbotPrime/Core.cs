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
    class Core
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string DatabaseName = "grabbotPrime";

        private const string ComponentsCollectionName = "components";

        private readonly IMongoClient _mongoClient;

        private Thread _tickThread;

        public Core(string databaseUri)
        {
            _mongoClient = new MongoClient(databaseUri);

            LoadComponents();
        }

        public bool Running { get; private set; } = false;

        private List<ComponentBase> Components { get; set; } = new List<ComponentBase>();

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
            where T : ComponentBase
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
            return (T)Components.Last();
        }

        public IEnumerable<T> GetComponents<T>()
            where T : ComponentBase
        {
            return Components.Where(x => x is T).Cast<T>();
        }

        public void LoadComponents()
        {
            Logger.Info("Loading components from database...");
            lock (Components)
            {
                Components.Clear();
                foreach (var document in RemoteComponentsCollection.FindAsync(Builders<BsonDocument>.Filter.Exists("uuid")).Result.ToEnumerable())
                {
                    Logger.Debug($"Component found: {document["type"].AsString}, {document["uuid"].AsString}");
                    Type componentType = ComponentRegistry.GetComponentTypeFromName(document["type"].AsString);
                    Components.Add((ComponentBase)Activator.CreateInstance(componentType, new object[] { RemoteComponentsCollection, document["uuid"].AsString }));
                    Components.Last().Core = this;
                    if (Running)
                    {
                        Components.Last().Init();
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
