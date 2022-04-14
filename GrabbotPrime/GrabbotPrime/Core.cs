using GrabbotPrime.Command;
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

        private readonly object _componentsLock = new object();

        public Core(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;

            LoadComponents();
        }

        public bool Running { get; private set; } = false;

        private List<IComponent> Components { get; set; } = new List<IComponent>();

        private Queue<ICommand> ContextualCommands { get; set; } = new Queue<ICommand>();

        private IEnumerable<ICommand> Commands { get; set; } = CommandsRegistry.GetCommands().ToList();

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
                component.Start();
            }

            foreach (var command in Commands)
            {
                command.Core = this;
            }

            Running = true;
            new Thread(TickLoop).Start();
            new Thread(TickLoopRare).Start();
        }

        public void TickLoop()
        {
            var timer = new Driscod.Audio.DriftTimer(TimeSpan.FromMilliseconds(20));

            while (Running)
            {
                foreach (var component in Components)
                {
                    try
                    {
                        component.Tick();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Component '{component}' threw an exception in the tick loop.");
                    }
                }
                timer.Wait().Wait();
            }
        }

        public void TickLoopRare()
        {
            var timer = new Driscod.Audio.DriftTimer(TimeSpan.FromSeconds(60));

            while (Running)
            {
                lock (_componentsLock)
                {
                    foreach (var component in Components)
                    {
                        try
                        {
                            component.TickRare();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"Component '{component}' threw an exception in the rare tick loop.");
                        }
                    }
                }

                PruneComponents();

                timer.Wait().Wait();
            }
        }

        public void PruneComponents()
        {
            lock (_componentsLock)
            {
                var destroy = new List<IComponent>();
                foreach (var component in Components)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", component.Id);
                    if (RemoteComponentsCollection.CountDocuments(filter) == 0)
                    {
                        destroy.Add(component);
                        Logger.Warn($"Component '{component}' does not exist in the database.");
                    }
                }

                foreach (var component in destroy)
                {
                    try
                    {
                        component.Stop();
                        Components.Remove(component);
                        Logger.Info($"Component '{component}' has been destroyed.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Failed to destroy component '{component}'.");
                    }
                }
            }
        }

        public T CreateComponent<T>(ObjectId? id = null, Action<T> preInitialization = null)
            where T : IComponent
        {
            if (id == null)
            {
                Components.Add((T)Activator.CreateInstance(typeof(T), new object[] { RemoteComponentsCollection, null }));
            }
            else
            {
                Components.Add((T)Activator.CreateInstance(typeof(T), new object[] { RemoteComponentsCollection, id }));
            }

            var component = (T)Components.Last();
            component.Core = this;

            if (Running)
            {
                preInitialization?.Invoke(component);
                component.Start();
            }

            Logger.Info($"Created component '{component}'.");

            return component;
        }

        public T CreateComponentIfNotExists<T>(ObjectId? id = null)
            where T : IComponent
        {
            if (id == null)
            {
                var component = Components.FirstOrDefault(x => x.GetType() == typeof(T));
                if (component != null)
                {
                    return (T)component;
                }
            }
            else
            {
                var component = Components.FirstOrDefault(x => x.Id == id);
                if (component != null)
                {
                    return (T)component;
                }
            }

            return CreateComponent<T>(id);
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
            lock (_componentsLock)
            {
                Components.Clear();
                foreach (var document in RemoteComponentsCollection.FindAsync(Builders<BsonDocument>.Filter.Empty).Result.ToEnumerable())
                {
                    Type componentType;
                    try
                    {
                        componentType = ComponentRegistry.GetComponentTypeFromName(document["type"].AsString);
                    }
                    catch (ArgumentException)
                    {
                        Logger.Error($"Unknown component type in component collection: '{document["type"].AsString}'");
                        continue;
                    }

                    Components.Add((ComponentBase)Activator.CreateInstance(componentType, new object[] { RemoteComponentsCollection, document["_id"].AsObjectId }));
                    var component = Components.Last();

                    component.Core = this;

                    Logger.Info($"Found component '{component}'.");

                    if (Running)
                    {
                        component.Start();
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
