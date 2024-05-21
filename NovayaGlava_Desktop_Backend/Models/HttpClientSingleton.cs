namespace NovayaGlava_Desktop_Backend.Models
{
    public class HttpClientSingleton
    {
        private static HttpClient _client;

        private static readonly object _lockObject = new object();

        private HttpClientSingleton() { }

        public static HttpClient Client
        {
            get
            {
                lock (_lockObject)
                {
                    if (_client != null)
                        return _client;
                    _client = new HttpClient();
                    return _client;
                }
            }
        }
    }
}
