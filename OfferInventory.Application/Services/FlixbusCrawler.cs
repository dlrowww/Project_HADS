// OfferInventory.Application/Services/FlixbusCrawler.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using OfferInventory.Domain.Entities;
using OfferInventory.Infrastructure.Data;

namespace OfferInventory.Application.Services;

/// <summary>
/// 调 FlixBus 公共 API，把班次写入数据库的爬虫服务。<br/>
/// 在 Program.cs 里先注册：<br/>
/// <c>builder.Services.AddHttpClient("flixbus");</c>
/// </summary>
public sealed class FlixbusCrawler
{
    private readonly AppDbContext _db;
    private readonly HttpClient   _http;

    // ——— 把 city / stationId ↔️ 人类可读名称，运行时可自动补充 ———
    private static readonly Dictionary<string, string> _cityDict = new()
    {
        ["40e19c59-8646-11e6-9066-549f350fcb0c"] = "Warsaw",
        ["40de6982-8646-11e6-9066-549f350fcb0c"] = "Gdańsk",
        ["40de7b94-8646-11e6-9066-549f350fcb0c"] = "Katowice",
        ["40de575f-8646-11e6-9066-549f350fcb0c"] = "Wrocław",
        ["40e19dd6-8646-11e6-9066-549f350fcb0c"] = "Łódź",
        ["40de7eb5-8646-11e6-9066-549f350fcb0c"] = "Kraków",
        ["40de8b67-8646-11e6-9066-549f350fcb0c"] = "Poznań",
        ["40de9afa-8646-11e6-9066-549f350fcb0c"] = "Szczecin",
        ["d7687a0f-488f-49c7-821e-5a26640c5f86"] = "Lublin",
        ["b5b11801-0928-43c1-af2e-2f01acbc0eca"] = "Białystok"
    };
    private static readonly Dictionary<string, string> _stationDict = new();

    // 仅用来把数字小数点都识别成 decimal
    private static readonly JsonDocumentOptions _docOpt = new()
    {
        AllowTrailingCommas = true,
        CommentHandling     = JsonCommentHandling.Skip
    };

    public FlixbusCrawler(AppDbContext db, IHttpClientFactory factory)
    {
        _db   = db;
        _http = factory.CreateClient("flixbus");
    }

    /// <summary>爬取“from ➜ to” 在 <paramref name="dateUtc"/> 那一天的全部班次。</summary>
    public async Task<int> CrawlAsync(string fromCityId, string toCityId, DateTime dateUtc)
    {
        if (dateUtc == default)
            dateUtc = DateTime.UtcNow.Date;

        var url  = BuildFlixbusUrl(fromCityId, toCityId, dateUtc);
        var json = await GetRawAsync(url);

        using var doc = JsonDocument.Parse(json, _docOpt);

        if (!doc.RootElement.TryGetProperty("trips", out var trips) ||
            trips.ValueKind != JsonValueKind.Array)
            return 0;

        var toInsert = new List<TransportOffer>();

        // ——— 遍历 trips ———
        foreach (var trip in trips.EnumerateArray())
        {
            if (!trip.TryGetProperty("results", out var results) ||
                results.ValueKind != JsonValueKind.Object)
                continue;

            // results = { "<key>": { …单个班次… }, … }
            foreach (var kv in results.EnumerateObject())
            {
                var obj = kv.Value;

                // ① uid → GUID
                if (!obj.TryGetProperty("uid", out var uidProp))
                    continue;

                var uidStr = uidProp.GetString();
                if (uidStr is null) continue;

                var guidPart = uidStr.Split(':', StringSplitOptions.RemoveEmptyEntries).Last();
                if (!Guid.TryParse(guidPart, out var tripId))
                    continue; // 解析失败

                // ② 同一次调用内去重 / 数据库去重
                if (toInsert.Any(t => t.Id == tripId) ||
                    await _db.TransportOffers.FindAsync(tripId) is not null)
                    continue;

                // ③ 必须字段 —— 如果缺失直接跳过
                if (!obj.TryGetProperty("departure", out var dep) ||
                    !obj.TryGetProperty("arrival",   out var arr))
                    continue;

                // ④ 尝试解析所有字段 —— 允许部分字段缺失
                var depDt = DateTime.Parse(
                    dep.GetProperty("date").GetString()!,
                    null, DateTimeStyles.AdjustToUniversal);
                var arrDt = DateTime.Parse(
                    arr.GetProperty("date").GetString()!,
                    null, DateTimeStyles.AdjustToUniversal);

                // optional
                var status       = obj.TryGetProperty("status",        out var st ) ? st.GetString() : "";
                var transferType = obj.TryGetProperty("transfer_type", out var tt ) ? tt.GetString() : "";
                var mot          = obj.TryGetProperty("means_of_transport", out var motProp)
                                 && motProp.ValueKind == JsonValueKind.String
                                 ? motProp.GetString()!
                                 : "bus";

                // price
                var priceTotal  = 0m;      // 必须有 total，否则跳过
                var priceAvg    = 0m;
                var priceOrig   = 0m;
                var currency    = "GBP";
                if (obj.TryGetProperty("price", out var price))
                {
                    if (!price.TryGetProperty("total", out var totalProp) ||
                        !totalProp.TryGetDecimal(out priceTotal))
                        continue; // total 缺失就没意义，直接下一个

                    price.TryGetProperty("average",  out var avgProp)  ;
                    price.TryGetProperty("original", out var origProp) ;
                    price.TryGetProperty("currency", out var curProp)  ;

                    if (avgProp  .ValueKind == JsonValueKind.Number) priceAvg  = avgProp .GetDecimal();
                    if (origProp .ValueKind == JsonValueKind.Number) priceOrig = origProp.GetDecimal();
                    if (curProp  .ValueKind == JsonValueKind.String) currency  = curProp .GetString() ?? currency;
                }

                // seats / duration / stops —— 都是可缺省
                var seatsAvail = obj.TryGetProperty("available", out var avail)
                               && avail.TryGetProperty("seats", out var seatsProp)
                               && seatsProp.TryGetInt32(out var s) ? s : 0;

                var durHours   = 0;
                var durMinutes = 0;
                if (obj.TryGetProperty("duration", out var dur))
                {
                    dur.TryGetProperty("hours"  , out var hProp);
                    dur.TryGetProperty("minutes", out var mProp);
                    hProp.TryGetInt32(out durHours);
                    mProp.TryGetInt32(out durMinutes);
                }

                var midStops = obj.TryGetProperty("intermediate_stations_count", out var stopProp) &&
                               stopProp.TryGetInt32(out var sc) ? sc : 0;

                // city / station
                var depCityId = dep.GetProperty ("city_id"   ).GetString()!;
                var depStaId  = dep.GetProperty ("station_id").GetString()!;
                var arrCityId = arr.GetProperty ("city_id"   ).GetString()!;
                var arrStaId  = arr.GetProperty ("station_id").GetString()!;

                var depCity = GetOrAdd(_cityDict,    depCityId, depCityId);
                var arrCity = GetOrAdd(_cityDict,    arrCityId, arrCityId);
                var depSta  = GetOrAdd(_stationDict, depStaId , depStaId );
                var arrSta  = GetOrAdd(_stationDict, arrStaId , arrStaId );

                // ⑤ 组装实体
                toInsert.Add(new TransportOffer
                {
                    Id                   = tripId,
                    Provider             = "FlixBus",
                    Status               = status,
                    TransferType         = transferType,

                    DepartureDate        = DateOnly .FromDateTime(depDt),
                    DepartureTime        = TimeOnly .FromDateTime(depDt),
                    ArrivalDate          = DateOnly .FromDateTime(arrDt),
                    ArrivalTime          = TimeOnly .FromDateTime(arrDt),

                    FromCity             = depCity,
                    FromStation          = depSta,
                    ToCity               = arrCity,
                    ToStation            = arrSta,

                    PriceTotal           = priceTotal,
                    PriceOriginal        = priceOrig,
                    PriceAverage         = priceAvg,
                    Currency             = currency,

                    SeatsAvailable       = seatsAvail,
                    DurationHours        = durHours,
                    DurationMinutes      = durMinutes,
                    IntermediateStations = midStops,
                    MeansOfTransport     = mot
                });
            }
        }

        // ——— 批量写库 ———
        if (toInsert.Count > 0)
        {
            _db.TransportOffers.AddRange(toInsert);
            await _db.SaveChangesAsync();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[DEBUG] Added {toInsert.Count} trip(s) to MySQL ✔");
        Console.ResetColor();

        return toInsert.Count;
    }

    /// <summary>构建 FlixBus 查询 URL（要求 <c>dd.MM.yyyy</c> 日期格式）。</summary>
    public static string BuildFlixbusUrl(string from, string to, DateTime dateUtc)
        => QueryHelpers.AddQueryString(
               "https://global.api.flixbus.com/search/service/v4/search",
               new Dictionary<string, string>
               {
                   ["from_city_id"]                = from,
                   ["to_city_id"]                  = to,
                   ["departure_date"]              = dateUtc.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                   ["products"]                    = "{\"adult\":1}",
                   ["currency"]                    = "GBP",
                   ["locale"]                      = "en_GB",
                   ["search_by"]                   = "cities",
                   ["include_after_midnight_rides"]= "1",
                   ["disable_distribusion_trips"]  = "0",
                   ["disable_global_trips"]        = "0"
               });

    // ——— 调试：打印请求与前 500 字符响应 ———
    public async Task<string> GetRawAsync(string url)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[DEBUG] Request  : {url}");
        Console.ResetColor();

        var req  = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; crawler)");

        var resp = await _http.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[DEBUG] Status   : {(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[DEBUG] Response : {body[..Math.Min(500, body.Length)]}…");
        Console.ResetColor();

        return body; // 不抛异常，方便诊断
    }

    private static string GetOrAdd(Dictionary<string,string> dict, string key, string fallback)
    {
        if (!dict.TryGetValue(key, out var name))
            dict[key] = name = fallback;
        return name;
    }
}
