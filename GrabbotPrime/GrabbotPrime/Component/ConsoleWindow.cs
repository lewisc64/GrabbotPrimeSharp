using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;

namespace GrabbotPrime.Component
{
    class ConsoleWindow : ComponentBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Thread _listenThread;

        public ConsoleWindow(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            _listenThread = new Thread(Listen);
            _listenThread.Start();
        }

        private void Listen()
        {
            while (true)
            {
                var userInput = Console.ReadLine();
                var command = Core.RecogniseCommand(userInput);
                command.Run(
                    userInput,
                    (message) =>
                    {
                        Console.WriteLine(message);
                    },
                    () =>
                    {
                        return Console.ReadLine();
                    });
            }
        }
    }
}
