namespace PPTWebApp.Data.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string SKU { get; set; }
        public ProductCategory? ProductCategory { get; set; }
        //Nullable in DB: indicates a software product (non physical)
        public ProductInventory? ProductInventory { get; set; }
        public decimal Price { get; set; }
        public Discount? Discount { get; set; }
        public required string ImageUrl { get; set; }
        // "horizontal" or "vertical". Possibly "auto"/"never"
        public required string ImageCompromise { get; set; }

        public bool IsPhysical()
        {
            return ProductInventory != null;
        }
    }
}
