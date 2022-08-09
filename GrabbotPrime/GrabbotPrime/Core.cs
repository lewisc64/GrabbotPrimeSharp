using GrabbotPrime.Command;
using GrabbotPrime.Component;
using GrabbotPrime.Device;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime
{
    public class Core
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string DatabaseName = "grabbotPrime";

        private const string ComponentsCollectionName = "components";

        private readonly IMongoClient _mongoClient;

        private readonly SemaphoreSlim _componentsSemaphore = new SemaphoreSlim(1);

        public Core(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public bool Running { get; private set; } = false;

        private List<IComponent> Components { get; set; } = new List<IComponent>();

        private Queue<ICommand> ContextualCommands { get; set; } = new Queue<ICommand>();

        private IEnumerable<ICommand> Commands { get; set; } = CommandsRegistry.GetCommands().ToList();

        private IMongoDatabase Database => _mongoClient.GetDatabase(DatabaseName);

        private IMongoCollection<BsonDocument> RemoteComponentsCollection => Database.GetCollection<BsonDocument>(ComponentsCollectionName);

        public async Task Start()
        {
            if (Running)
            {
                throw new InvalidOperationException("Core is already running.");
            }
            Logger.Info("Starting core...");

            foreach (var component in Components)
            {
                await component.Start();
            }

            foreach (var command in Commands)
            {
                command.Core = this;
            }

            Running = true;
            new Thread(async () => await TickLoop()).Start();
            new Thread(async () => await TickLoopRare()).Start();
        }

        public async Task TickLoop()
        {
            var timer = new Driscod.Audio.DriftTimer(TimeSpan.FromMilliseconds(20));

            while (Running)
            {
                foreach (var component in Components)
                {
                    try
                    {
                        await component.Tick();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Component '{component}' threw an exception in the tick loop.");
                    }
                }
                timer.Wait().Wait();
            }
        }

        public async Task TickLoopRare()
        {
            var timer = new Driscod.Audio.DriftTimer(TimeSpan.FromSeconds(60));

            while (Running)
            {
                await _componentsSemaphore.WaitAsync();
                try
                {
                    foreach (var component in Components)
                    {
                        try
                        {
                            await component.TickRare();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"Component '{component}' threw an exception in the rare tick loop.");
                        }
                    }
                }
                finally
                {
                    _componentsSemaphore.Release();
                }

                await PruneComponents();
                await timer.Wait();
            }
        }

        public async Task PruneComponents()
        {
            await _componentsSemaphore.WaitAsync();
            try
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
                        await component.Stop();
                        Components.Remove(component);
                        Logger.Info($"Component '{component}' has been destroyed.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Failed to destroy component '{component}'.");
                    }
                }
            }
            finally
            {
                _componentsSemaphore.Release();
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

        public async Task LoadComponentsFromDatabase()
        {
            Logger.Info("Loading components from database...");
            await _componentsSemaphore.WaitAsync();
            try
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
                        await component.Start();
                    }
                }
            }
            finally
            {
                _componentsSemaphore.Release();
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
