using MongoDB.Bson;
using System.Net.Http;

namespace Phew
{
    public class Light
    {
        private bool _on;

        private int _brightness;

        private int _saturation;

        private int _hue;

        public int Number { get; private set; }

        public string Name { get; private set; }

        public Bridge Bridge { get; private set; }

        public bool On
        {
            get
            {
                return _on;
            }
            set
            {
                _on = value;
                UpdateState(new BsonDocument
                {
                    { "on", _on },
                });
            }
        }

        public double Brightness // %
        {
            get
            {
                return 254D / _brightness;
            }
            set
            {
                _brightness = (int)(254 * value / 100);
                UpdateState(new BsonDocument
                {
                    { "bri", _brightness },
                });
            }
        }

        public double Saturation // %
        {
            get
            {
                return 254D / _saturation;
            }
            set
            {
                _saturation = (int)(254 * value / 100);
                UpdateState(new BsonDocument
                {
                    { "sat", _saturation },
                });
            }
        }

        public double Hue // degrees
        {
            get
            {
                return _hue / 65535D * 360;
            }
            set
            {
                _hue = (int)(65535 * ((value % 360) / 360));
                UpdateState(new BsonDocument
                {
                    { "hue", _hue },
                });
            }
        }

        public Light(Bridge bridge, int number)
        {
            Bridge = bridge;
            Number = number;
        }

        public void SetFromDocument(BsonDocument document)
        {
            _on = document["state"]["on"].AsBoolean;
            _brightness = document["state"]["bri"].AsInt32;
            _hue = document["state"]["bri"].AsInt32;
            _saturation = document["state"]["sat"].AsInt32;

            Name = document["name"].AsString;
        }

        private BsonValue UpdateState(BsonDocument data)
        {
            return Bridge.SendApiRequest(HttpMethod.Put, $"api/{Bridge.Username}/lights/{Number}/state", data);
        }
    }
}
