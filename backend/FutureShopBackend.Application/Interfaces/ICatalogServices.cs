using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;

namespace FutureShopBackend.Application.Interfaces;

/// <summary>
/// Application service contract for hierarchical categories and dynamic category specifications.
/// </summary>
public interface ICategoryService
{
    Task<IReadOnlyList<CategoryTreeDto>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryAttributeDefinitionDto>> GetCategoryAttributesAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<CategoryTreeDto> CreateCategoryAsync(UpsertCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryTreeDto> UpdateCategoryAsync(Guid categoryId, UpsertCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for catalog brands.
/// </summary>
public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(UpsertBrandRequest request, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateBrandAsync(Guid brandId, UpsertBrandRequest request, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(Guid brandId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for catalog search, filtering, quick view, price history, product comparison, and seller CRUD.
/// </summary>
public interface IProductService
{
    Task<PagedResponse<ProductCardDto>> SearchProductsAsync(CatalogSearchQuery query, CancellationToken cancellationToken = default);
    Task<SearchSuggestionsResponse> GetSearchSuggestionsAsync(string query, int limit = 8, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductCardDto>> GetRecommendationsAsync(int limit = 12, CancellationToken cancellationToken = default);
    Task<ProductComparisonResponse> CompareProductsAsync(IReadOnlyList<Guid> productIds, bool onlyDifferences = false, CancellationToken cancellationToken = default);
    Task<ProductDetailsResponse> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductQuickViewResponse> GetProductQuickViewAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductPriceHistoryResponse> GetProductPriceHistoryAsync(Guid productId, int periodDays = 90, CancellationToken cancellationToken = default);
    Task<ProductDetailsResponse> CreateProductAsync(UpsertProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDetailsResponse> UpdateProductAsync(Guid productId, UpsertProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDetailsResponse> UpdateProductStatusAsync(Guid productId, UpdateProductStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductStatisticsResponse> GetProductStatisticsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<BulkProductImportResultResponse> ImportProductsAsync(BulkProductImportRequest request, CancellationToken cancellationToken = default);
}
