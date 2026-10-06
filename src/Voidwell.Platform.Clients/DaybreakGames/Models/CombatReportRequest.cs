namespace Voidwell.Platform.Clients.DaybreakGames.Models;

public class CombatReportRequest
{
    public string? WorldId { get; set; }
    public string? ZoneId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
