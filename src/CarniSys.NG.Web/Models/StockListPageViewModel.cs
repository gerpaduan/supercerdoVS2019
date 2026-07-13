using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Stock;

namespace CarniSys.NG.Web.Models;

public sealed class StockListPageViewModel
{
    public bool CanManageStock { get; init; }

    public static readonly IReadOnlyCollection<string> DefaultStateOptions =
    [
        "Todos",
        "OK",
        "BAJO",
        "SIN STOCK",
        "NEGATIVO"
    ];

    public string SearchText { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public DateTime UntilDate { get; init; }

    public string Type { get; init; } = string.Empty;

    public bool OnlyWithStock { get; init; }

    public string StockState { get; init; } = "Todos";

    public required IReadOnlyCollection<BranchLookupItem> Branches { get; init; }

    public required IReadOnlyCollection<string> StateOptions { get; init; }

    public required StockMatrixResult Result { get; init; }
}
