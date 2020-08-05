using System.Collections.Generic;
using System.Linq;
using GrabbotPrime.Component;
using GrabbotPrime.Device;
using GrabbotPrime.Integrations.PhilipsHue.Devices;
using MongoDB.Bson;
using MongoDB.Driver;
using Phew;

namespace GrabbotPrime.Integrations.PhilipsHue
{
    public partial class HueBridge : ComponentBase, IHasDevices
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string BridgeId
        {
            get
            {
                return GetPropertyByName("bridgeId")?.AsString;
            }

            set
            {
                SetPropertyByName("bridgeId", value);
            }
        }

        public string Username
        {
            get
            {
                return GetPropertyByName("username")?.AsString;
            }

            set
            {
                SetPropertyByName("username", value);
            }
        }

        private Bridge Bridge { get; set; }

        public HueBridge(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();

            if (BridgeId == null)
            {
                Logger.Fatal("ID of bridge is not set.");
                return;
            }

            if (Username == null)
            {
                Logger.Fatal("Username for API connection not set.");
                return;
            }

            Bridge = new Bridge(BridgeId, Username);
        }

        public IEnumerable<IDevice> GetDevices()
        {
            return Bridge.GetLights().Select(x => new HueLight(x));
        }
    }
}
