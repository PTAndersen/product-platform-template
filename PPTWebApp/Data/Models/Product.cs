namespace PPTWebApp.Data.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        // "horizontal" or "vertical". Possibly "auto"/"never"
        public string ImageCompromise { get; set; }
    }
}
