using System;

namespace OfferInventory.Domain.Entities;

/// <summary>
/// 统一保存 FlixBus（或其他供应商）车次信息的领域实体。
/// </summary>
public class TransportOffer
{
    public Guid Id { get; set; }                         // Trip-UUID —— 主键

    // ───── 供应商 & 班次基本状态 ─────
    public string Provider      { get; set; } = string.Empty;   // flixbus
    public string Status        { get; set; } = string.Empty;   // available / sold_out …
    public string TransferType  { get; set; } = string.Empty;   // Fast / Slow …

    // ───── 出发 / 到达 ─────
    public DateOnly DepartureDate { get; set; }                 // 2025-04-23
    public TimeOnly DepartureTime { get; set; }                 // 18:00
    public DateOnly ArrivalDate   { get; set; }
    public TimeOnly ArrivalTime   { get; set; }

    // ───── 城市 & 车站 ─────
    public string FromCity       { get; set; } = string.Empty;  // Warsaw
    public string FromStation    { get; set; } = string.Empty;  // Metro Mlociny
    public string ToCity         { get; set; } = string.Empty;
    public string ToStation      { get; set; } = string.Empty;

    // ───── 价格 ─────
    public decimal PriceTotal    { get; set; }                  // 最终成交价
    public decimal PriceOriginal { get; set; }                  // 原价
    public decimal PriceAverage  { get; set; }                  // 均价
    public string  Currency      { get; set; } = "USD";

    // ───── 行程 & 座位 ─────
    public int DurationHours          { get; set; }             // 4
    public int DurationMinutes        { get; set; }             // 15
    public int IntermediateStations   { get; set; }             // 0,1,2…
    public int SeatsAvailable         { get; set; }             // 可订座位
    public string MeansOfTransport    { get; set; } = "bus";    // bus / train …

    public bool IsPromoted { get; set; } = false;               // 促销标记
}
