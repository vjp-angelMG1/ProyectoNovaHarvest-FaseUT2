using Microsoft.AspNetCore.Mvc.RazorPages;
using NovaHarvest.Web.Models;

namespace NovaHarvest.Web.Pages;

public class StatusModel : PageModel
{
    public DateTime ServerTime { get; private set; }
    public string ServerName { get; private set; } = "";
    public int Visits { get; private set; }

    public void OnGet()
    {
        ServerTime = DateTime.Now;
        ServerName = Environment.MachineName;
        SiteStats.Visits++;
        Visits = SiteStats.Visits;
    }
}

public class TicketModel : PageModel
{
    public string Reservation { get; private set; } = "RES-0001";
    public string Product { get; private set; } = "Cereza Picota";
    public int Kg { get; private set; } = 20;
    public string Pickup { get; private set; } = "Cooperativa del Valle del Jerte";
    public DateTime GeneratedAt { get; private set; }

    public void OnGet()
    {
        GeneratedAt = DateTime.Now;
    }
}