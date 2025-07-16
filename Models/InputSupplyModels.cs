namespace AgriSmartAPI.Models;

public class InputSupplier
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ProductType { get; set; }
    public double Rating { get; set; }
    public List<string> Reviews { get; set; } = new List<string>();
}

public class InputOrder
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string SupplierId { get; set; }
    public string Product { get; set; }
    public double Quantity { get; set; }
    public string DeliveryLocation { get; set; }
    public string Status { get; set; }
}