using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace YoutubeConverter.Services
{
    internal class HtmlService
    {
        /// <summary>
        /// Makes a GET request to an api to retrieve the mpd link.
        /// It then parses the response to return only the link.
        /// </summary>
        public static async Task<string> GetMpdLinkFromUserUrl(string userUrl)
        {
            var number = ExtractLastNumber(userUrl);
            if (number == null)
            {
                return null;
            }

            HttpClient httpClient = new HttpClient();

            var apiUrl = $"https://vod.tvp.pl/api/products/{number}/videos/playlist?platform=BROWSER&videoType=MOVIE";
            var response = await httpClient.GetStringAsync(apiUrl);

            var json = JObject.Parse(response);
            var dashUrl = json["sources"]?["DASH"]?[0]?["src"]?.ToString();
            return dashUrl;
        }

        /// <summary>
        /// Extracts the last number from a string
        /// </summary>
        private static string ExtractLastNumber(string url)
        {
            var match = System.Text.RegularExpressions.Regex.Match(url, @"(\d+)(?!.*\d)");
            return match.Success ? match.Value : null;
        }
    }
}