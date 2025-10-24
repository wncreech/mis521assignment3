namespace Betterboxd.Models
{
    public class SentimentViewModel
    {
        //toplevel info
        public string QueryTitle { get; set; }
        public List<Comment> Comments { get; set; }
        public string OverallSentiment { get; set; }
        public double AverageScore { get; set; }

        public SentimentViewModel()
        {
            Comments = new List<Comment>();
        }
    }
}
