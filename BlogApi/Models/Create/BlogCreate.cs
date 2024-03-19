namespace BlogApi.Models.Create
{
    public class BlogCreate
    {
        public string Title { get; set; } = null!;

        public string Body { get; set; } = null!;

        public int CategoryId { get; set; }
    }
}
