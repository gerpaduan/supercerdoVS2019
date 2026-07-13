namespace CarniSys.NG.Application.Stock;

public interface IStockQueryService
{
    Task<StockMatrixResult> GetStockMatrixAsync(
        int companyId,
        StockMatrixQuery query,
        CancellationToken cancellationToken = default);

    Task<StockEditDetailItem?> GetStockEditByIdAsync(
        int companyId,
        int stockId,
        CancellationToken cancellationToken = default);

    Task<StockWeighingPercentagesItem?> GetWeighingPercentagesAsync(
        int companyId,
        int stockId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockWeighingPurchaseLookupItem>> GetWeighingPurchasesAsync(
        int companyId,
        StockWeighingPurchaseQuery query,
        CancellationToken cancellationToken = default);

    Task<StockWeighingPurchaseLookupItem?> GetWeighingPurchaseByIdAsync(
        int companyId,
        int stockId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockMissingClosingProductItem>> GetMissingClosingProductsAsync(
        int companyId,
        int branchId,
        DateTime operationDate,
        IReadOnlyCollection<long> loadedCodes,
        CancellationToken cancellationToken = default);
}
