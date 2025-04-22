using OfferInventory.Domain.Entities;

namespace OfferInventory.Application.Interfaces;

public interface IOfferService
{
    /// <summary>
    /// 生成 count 条随机假数据到数据库，返回实际插入数量
    /// </summary>
    Task<int> SeedAsync(int count, CancellationToken ct = default);

    /// <summary>
    /// 获取所有 transportOffer
    /// </summary>
    Task<List<TransportOffer>> GetAllAsync(CancellationToken ct = default);
}

