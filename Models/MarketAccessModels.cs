namespace AgriSmartAPI.Models;

public class MarketPrice
{
    public int Id { get; set; }
    public string CropName { get; set; }
    public string MarketLocation { get; set; }
    public double Price { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class MarketplaceListing
{
    public int Id { get; set; }
    public string SellerId { get; set; }
    public string CropName { get; set; }
    public double Quantity { get; set; }
    public double AskingPrice { get; set; }
    public string BuyerId { get; set; }
}

public class GroupSale
{
    public int Id { get; set; }
    public string CooperativeId { get; set; }
    public string CropName { get; set; }
    public double TotalQuantity { get; set; }
    public double NegotiatedPrice { get; set; }
}