namespace NovaHarvest.Web.Models;

public class Surplus(string product, string producer, string village, DateTime harvestDate,
    int availableKg, int reservedKg, decimal pricePerKg, string? note)
{
    public string Product { get; init; } = product;
    public string Producer { get; init; } = producer;
    public string Village { get; init; } = village;
    public DateTime HarvestDate { get; init; } = harvestDate;
    public int AvailableKg { get; init; } = availableKg;
    public int ReservedKg { get; init; } = reservedKg;
    public decimal PricePerKg { get; init; } = pricePerKg;
    public string? Note { get; init; } = note;
}