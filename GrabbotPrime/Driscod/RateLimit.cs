using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace Driscod
{
    public class RateLimit
    {
        private object _lock = new object();

        public string Id { get; private set; }

        public int Max { get; private set; }

        public int Remaining { get; private set; }

        public DateTime? ResetAt { get; private set; }

        public RateLimit(string id)
        {
            Id = id;
        }

        public void UpdateFromHeaders(HttpResponseHeaders headers)
        {
            if (headers.Contains("X-RateLimit-Reset"))
            {
                ResetAt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(long.Parse(headers.GetFirstValue("X-RateLimit-Reset")));
            }
            if (headers.Contains("X-RateLimit-Limit"))
            {
                Max = int.Parse(headers.GetFirstValue("X-RateLimit-Limit"));
            }
            if (headers.Contains("X-RateLimit-Remaining"))
            {
                Remaining = int.Parse(headers.GetFirstValue("X-RateLimit-Remaining"));
            }
        }

        public void LockAndWait(Func<HttpResponseMessage> callback)
        {
            lock (_lock)
            {
                HttpResponseMessage response;
                int retryAfter = -1;
                do
                {
                    if (retryAfter != -1)
                    {
                        Thread.Sleep(retryAfter);
                        retryAfter = -1;
                    }
                    if (Remaining == 0 && ResetAt != null)
                    {
                        Thread.Sleep((DateTime)ResetAt - DateTime.UtcNow);
                    }

                    response = callback();
                    UpdateFromHeaders(response.Headers);
                }
                while (response.StatusCode == (System.Net.HttpStatusCode)429);
            }
        }
    }
}
