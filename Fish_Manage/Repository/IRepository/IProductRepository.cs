using Fish_Manage.Models;

namespace Fish_Manage.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> UpdateAsync(Product entity);
        Task<List<Product>> GetProductAsc(decimal minRange, decimal maxRange);
        Task<List<Product>> GetProductDesc(decimal minRange, decimal maxRange);
        Task<List<Product>> GetProductNewest(decimal minRange, decimal maxRange);
        Task<List<Product>> GetProductOldest(decimal minRange, decimal maxRange);
        Task<List<Product>> GetProductInRange(decimal minRange, decimal maxRange);

        int? GetQuantity(string id);
        Task<Product> GetByIdAsync(string id);
        Task<List<Product>> GetByIdsAsync(List<string> ids);
        Task UpdateRangeAsync(List<Product> products);
    }
}
