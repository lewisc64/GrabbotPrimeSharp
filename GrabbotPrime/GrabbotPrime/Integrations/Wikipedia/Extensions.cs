using System;
using System.Net;

namespace GrabbotPrime.Integrations.Wikipedia
{
    public static class HttpStatusCodeExtensions
    {
        public static void ThrowIfNot(this HttpStatusCode statusCode, HttpStatusCode desired)
        {
            if (statusCode != desired)
            {
                throw new InvalidOperationException($"Expected status code '{desired}', but recieved '{statusCode}'.");
            }
        }
    }
}
