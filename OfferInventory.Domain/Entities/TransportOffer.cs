using System;

namespace OfferInventory.Domain.Entities
{
    /// <summary>
    /// 统一保存 FlixBus（或其他供应商）车次信息的领域实体。
    /// </summary>
    public class TransportOffer
    {
        public Guid   Id                 { get; set; }           // Trip-UUID —— 主键
        public string Provider           { get; set; } = string.Empty;
        public string Status             { get; set; } = string.Empty;
        public string TransferType       { get; set; } = string.Empty;

        public DateOnly DepartureDate    { get; set; }
        public TimeOnly DepartureTime    { get; set; }
        public DateOnly ArrivalDate      { get; set; }
        public TimeOnly ArrivalTime      { get; set; }

        public string FromCity           { get; set; } = string.Empty;
        public string FromStation        { get; set; } = string.Empty;
        public string ToCity             { get; set; } = string.Empty;
        public string ToStation          { get; set; } = string.Empty;

        public decimal PriceTotal        { get; set; }
        public decimal PriceOriginal     { get; set; }
        public decimal PriceAverage      { get; set; }
        public string  Currency          { get; set; } = "USD";

        public int     DurationHours     { get; set; }
        public int     DurationMinutes   { get; set; }
        public int     IntermediateStations { get; set; }
        public int     SeatsAvailable    { get; set; }
        public string  MeansOfTransport  { get; set; } = "bus";

        public bool    IsPromoted        { get; set; } = false;

        // —— 新增这一行 —— 存储刚才生成的 8 位哈希短 ID
        public string  ShortId           { get; set; } = string.Empty;
    }
}
