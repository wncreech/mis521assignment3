using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Betterboxd.Models; // Make sure your Comment class is in Betterboxd.Models

namespace Betterboxd.Services
{
    public class SentimentService
    {
        private const int MaxInputLength = 512;
        private readonly string huggingFaceApiKey;

        public SentimentService(string apiKey)
        {
            huggingFaceApiKey = apiKey;
        }

        public static string TruncateToMaxLength(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength);
        }

        public async Task<(List<Comment> Results, string Overall, double Average)> AnalyzeSentimentForQueryAsync(string searchQuery, string type = "movie")
        {
            var results = new List<Comment>();
            var url = "https://router.huggingface.co/hf-inference/models/distilbert/distilbert-base-uncased-finetuned-sst-2-english";

            List<string> textToExamine = await SearchRedditAsync(searchQuery + " " + type);
            double totalScore = 0;
            int validResponses = 0;

            using (var httpClient = new HttpClient())
            {
                foreach (var post in textToExamine)
                {
                    var data = new { inputs = new[] { post } };
                    var json = JsonSerializer.Serialize(data);

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(url),
                        Headers = { { "Authorization", $"Bearer {huggingFaceApiKey}" } },
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    var response = await httpClient.SendAsync(request);
                    var responseString = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var sentimentResults = JsonSerializer.Deserialize<List<List<SentimentResponse>>>(responseString);

                        if (sentimentResults != null && sentimentResults.Count > 0 && sentimentResults[0].Count > 0)
                        {
                            var result = sentimentResults[0][0]; // take the first sentiment for this post
                            double score = result.Score;
                            if (result.Label == "NEGATIVE") score *= -1;
                            totalScore += score;
                            validResponses++;

                            results.Add(new Comment
                            {
                                Text = post,
                                Label = result.Label,
                                Score = score
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Optionally log or Debug.WriteLine(ex.Message);
                    }
                }
            }

            double averageScore = validResponses > 0 ? totalScore / validResponses : 0;
            string overall = averageScore >= 0 ? "POSITIVE" : "NEGATIVE";

            return (results, overall, averageScore);
        }

        // Fetch top 25 Reddit comments for a movie/actor
        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0");

                string json = await client.GetStringAsync(
                    "https://api.pullpush.io/reddit/search/comment/?size=25&q=" + HttpUtility.UrlEncode(searchQuery)
                );

                JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("data", out JsonElement dataArray))
                {
                    foreach (var comment in dataArray.EnumerateArray())
                    {
                        if (comment.TryGetProperty("body", out JsonElement bodyElement))
                        {
                            string textToAdd = bodyElement.GetString();
                            if (!string.IsNullOrEmpty(textToAdd))
                            {
                                textToAdd = TruncateToMaxLength(textToAdd, MaxInputLength);
                                returnList.Add(textToAdd);
                            }
                        }
                    }
                }
            }

            return returnList;
        }
    }

    //deserialize of HuggingFace response
    public class SentimentResponse
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }
    }
}
