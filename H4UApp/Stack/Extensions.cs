using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace H4UApp.Stack
{
    public static class Extensions
    {
        public static T GetObject<T>(this HttpResponseMessage response)
        {
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string GetString(this HttpResponseMessage response)
        {
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
