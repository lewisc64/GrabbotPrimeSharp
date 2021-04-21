using GrabbotPrime.Commands;
using GrabbotPrime.Component;
using GrabbotPrime.Device;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        private Queue<ICommand> ContextualCommands { get; set; } = new Queue<ICommand>();

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
            var timer = new Driscod.Audio.DriftTimer(TimeSpan.FromMilliseconds(20));

            while (Running)
            {
                foreach (var component in Components)
                {
                    component.Tick();
                }
                timer.Wait().Wait();
            }
        }

        public T CreateComponent<T>(string uuid = null, Action<T> preInitialization = null)
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

            var component = (T)Components.Last();
            component.Core = this;

            if (Running)
            {
                preInitialization?.Invoke(component);
                component.Init();
            }

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

        public IEnumerable<IDevice> GetDevices()
        {
            return GetComponents<IHasDevices>()
                .Select(x => x.GetDevices())
                .Aggregate(new List<IDevice>(), (acc, devices) => { acc.AddRange(devices); return acc; });
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

        public void AddContextualCommand(ICommand command)
        {
            if (!(command is IContextualCommand))
            {
                throw new ArgumentException("Command is not contextual.", nameof(command));
            }

            ContextualCommands.Enqueue(command);
        }

        public ICommand RecogniseCommand(string command)
        {
            try
            {
                while (ContextualCommands.Any())
                {
                    var commandInstance = ContextualCommands.Dequeue();
                    if (commandInstance.Recognise(command))
                    {
                        return commandInstance;
                    }
                }
            }
            finally
            {
                ContextualCommands.Clear();
            }

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
