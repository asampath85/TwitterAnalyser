using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyser
{
    class Program
    {

        private const string TwitterConsumerKey = "yeztpKZcCqNBQLEWoondDcvH7";
        private const string TwitterConsumerSecret = "0kqgCZ1ZzJUHk7VO7XkYwonKVYVxIFX9n5xmgXbBlDHrvdZHVk";
        static void Main(string[] args)
        {
            ExtractTweetsByGeoCode();
        }

        private static string GetAccessToken()
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://api.twitter.com/oauth2/token ");
            var customerInfo = Convert.ToBase64String(new UTF8Encoding()
                                      .GetBytes(TwitterConsumerKey + ":" + TwitterConsumerSecret));
            request.Headers.Add("Authorization", "Basic " + customerInfo);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8,
                                                                      "application/x-www-form-urlencoded");

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            string json = response.Content.ReadAsStringAsync().Result;
            dynamic item = JsonConvert.DeserializeObject<object>(json);
            return item["access_token"];
        }

        private static void ExtractTweetsByGeoCode()
        {
            string url = "";
            var max_id = "";
            double lattitude = 39.2903848;
            double longitude = -76.61218930000001;
            bool isGeo = true;

            if (isGeo)
            {
                url = string.Format("https://api.twitter.com/1.1/search/tweets.json?geocode={0},{1},1000mi&q=%23FreddieGray&count=100", lattitude, longitude);
            }
            else
            {

                url = string.Format("https://api.twitter.com/1.1/search/tweets.json?q=%23FreddieGray&count=100");
            }

            var requestUserTimeline = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            var accessToken = GetAccessToken();
            requestUserTimeline.Headers.Add("Authorization", "Bearer " + accessToken);
            var httpClient = new HttpClient();
            HttpResponseMessage responseUserTimeLine = httpClient.SendAsync(requestUserTimeline).Result;

            dynamic jsonResponse = JsonConvert.DeserializeObject<object>(responseUserTimeLine.Content.ReadAsStringAsync().Result);

            var enumerableTweets = (jsonResponse.statuses as IEnumerable<dynamic>);

            List<TweetModel> modelList = new List<TweetModel>();
            foreach (var item in enumerableTweets)
            {
                modelList.Add(new TweetModel
                {
                    Text = item.text.Value,
                    PostedBy = item.user.name.Value,
                    PostedAt = item.created_at.Value,
                    ProfileImageURL = item.user.profile_image_url_https.Value,
                    TweetIdStr = item.id_str.Value,
                    Id = item.id,
                    Lattitude = isGeo != true ? 0 : (item.geo != null ? Convert.ToDecimal(item.geo.coordinates[0].Value) : Convert.ToDecimal(item.retweeted_status.geo.coordinates[0].Value)),
                    Longitude = isGeo != true ? 0 : (item.geo != null ? Convert.ToDecimal(item.geo.coordinates[1].Value) : Convert.ToDecimal(item.retweeted_status.geo.coordinates[1].Value))
                });

            }

            var min_id = (modelList.Min(x => x.Id)-1).ToString();
            var since_id = modelList.Max(x => x.Id).ToString();

            var resultcount = enumerableTweets.Count();

            while(resultcount==100)
            {
                if (isGeo)
                {
                    url = string.Format("https://api.twitter.com/1.1/search/tweets.json?geocode={1},{2},1000mi&q=%23FreddieGray&count=100&max_id={0}", min_id, lattitude, longitude);
                }
                else
                {
                    url = string.Format("https://api.twitter.com/1.1/search/tweets.json?q=%23FreddieGray&count=100&max_id={0}", min_id);
                }

                var requestUserTimeline1 = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                requestUserTimeline1.Headers.Add("Authorization", "Bearer " + accessToken);
                var httpClient1 = new HttpClient();
                HttpResponseMessage responseUserTimeLine1 = httpClient1.SendAsync(requestUserTimeline1).Result;

                jsonResponse = JsonConvert.DeserializeObject<object>(responseUserTimeLine1.Content.ReadAsStringAsync().Result);
                enumerableTweets = (jsonResponse.statuses as IEnumerable<dynamic>);

                foreach (var item in enumerableTweets)
                {
                    modelList.Add(new TweetModel
                    {
                        Text = item.text.Value,
                        PostedBy = item.user.name.Value,
                        PostedAt = item.created_at.Value,
                        ProfileImageURL = item.user.profile_image_url_https.Value,
                        TweetIdStr = item.id_str.Value,
                        Id = item.id,
                        Lattitude = isGeo != true ? 0 : (item.geo != null ? Convert.ToDecimal(item.geo.coordinates[0].Value) : Convert.ToDecimal(item.retweeted_status.geo.coordinates[0].Value)),
                        Longitude = isGeo != true ? 0 : (item.geo != null ? Convert.ToDecimal(item.geo.coordinates[1].Value) : Convert.ToDecimal(item.retweeted_status.geo.coordinates[1].Value))
                    });

                }

                min_id = modelList.Min(x => x.Id).ToString();
                //since_id = modelList.Max(x => x.Id).ToString();

                resultcount = enumerableTweets.Count();
                Console.WriteLine("Received " + modelList.Count().ToString() + " From Twitter");
               

            }

            //return modelList;
            Console.ReadLine();


        }
    }

    public class TweetModel
    {
        public decimal Id { get; set; }
        public string Text { get; set; }
        public string PostedBy { get; set; }
        public string PostedAt { get; set; }
        public Nullable<double> Score { get; set; }
        public string ProfileImageURL { get; set; }
        public string TweetIdStr { get; set; }
        public int RequestId { get; set; }
        public bool IsHighlight { get; set; }
        public decimal Lattitude { get; set; }
        public decimal Longitude { get; set; }

    }
}
