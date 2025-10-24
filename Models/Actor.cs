namespace Betterboxd.Models
{
    public class Actor
    {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public int Age { get; set; }
            public string ImdbUrl { get; set; }
            public string PhotoUrl { get; set; }

            public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
    }
