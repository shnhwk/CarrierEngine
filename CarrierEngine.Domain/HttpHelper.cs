using System.Net.Http.Headers;
using System.Text;

namespace CarrierEngine.Domain
{
    public static class HttpHelper
    {

        public static string GetContentType(HttpHeaders headers)
        {
            if (headers is null)
                return string.Empty;

            return headers.TryGetValues("Content-Type", out var values)
                ? values.GetEnumerator().Current
                : string.Empty;
        }

        public static string GetHeaderString(HttpHeaders headers)
        {
            if (headers is null)
                return string.Empty;


            var stringBuilder = new StringBuilder();

            foreach (var header in headers)
            {
                stringBuilder.Append($"{header.Key}:{string.Join(",", header.Value)}|");
            }

            return stringBuilder.ToString().TrimEnd('|');
        }


    }
}
