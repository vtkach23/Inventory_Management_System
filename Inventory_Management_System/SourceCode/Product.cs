// data model to define what a product is
namespace simpleInventoryGUI // namespace to group related class
{
    public class Product // define public class to represent Product item in inventory
    {
        // public string Id { get; set; } (Could be used in the future)
        public string Name { get; set; } // name of product
        public string Barcode { get; set; } // barcode identifier
        public int Quantity { get; set; } // quantity identifier
        public string Supplier { get; set; } // supplier identifier
    }
}
