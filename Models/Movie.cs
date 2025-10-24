namespace Betterboxd.Models
{
    public class Movie
    {
        public int id { get; set; }
        public string title { get; set; }
        public string genre { get; set; }
        public int year { get; set; }
        public string imdbUrl { get; set; }
        public string posterUrl { get; set; }

        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
    }
}
