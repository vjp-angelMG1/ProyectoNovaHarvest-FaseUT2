using Microsoft.AspNetCore.Mvc.RazorPages;
using NovaHarvest.Web.Models;

namespace NovaHarvest.Web.Pages;

public class SurplusModel : PageModel
{
    private const decimal SurplusDiscount = 0.30m;
    private const int BoxKg = 8;
    private const int ShelfLifeDays = 7;
    private const int LowStockKg = 50;

    public Surplus Surplus { get; } = new("Cereza Picota", "Huerta Los Castaños",
        "Cabezuela del Valle", DateTime.Today.AddDays(-2), 343, 120, 3.20m,
        "<b>Recoger antes de las 14:00</b>");

    public DateTime ExpiryDate { get; private set; }
    public int DaysRemaining { get; private set; }
    public decimal SurplusPrice { get; private set; }
    public int RemainingKg { get; private set; }
    public int BoxCount { get; private set; }
    public int LooseKg { get; private set; }
    public int BoxSize { get; private set; }
    public decimal SurplusValue { get; private set; }
    public string State { get; private set; } = "";
    public string? UrgentClass { get; private set; }
    public bool SoldOut { get; private set; }

    public void OnGet()
    {
        ExpiryDate = Surplus.HarvestDate.AddDays(ShelfLifeDays);
        DaysRemaining = (ExpiryDate - DateTime.Today).Days;
        SurplusPrice = Surplus.PricePerKg * (1 - SurplusDiscount);
        RemainingKg = Surplus.AvailableKg - Surplus.ReservedKg;
        BoxCount = RemainingKg / BoxKg;
        LooseKg = RemainingKg % BoxKg;
        BoxSize = BoxKg;
        SurplusValue = SurplusPrice * RemainingKg;
        SoldOut = RemainingKg == 0;
        State = SoldOut ? "Agotado" : (RemainingKg < LowStockKg ? "¡Últimos kilos!" : "Disponible");
        UrgentClass = DaysRemaining <= 2 && RemainingKg > 0 ? "urgent" : null;
    }
}
