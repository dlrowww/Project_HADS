// OfferInventory.Application/Services/FlixbusCrawler.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using OfferInventory.Domain.Entities;
using OfferInventory.Infrastructure.Data;

namespace OfferInventory.Application.Services;

/// <summary>
/// 调 FlixBus 公共 API，把班次写入数据库的爬虫服务。
/// 在 Program.cs 里先注册： builder.Services.AddHttpClient("flixbus");
/// </summary>
public sealed class FlixbusCrawler
{
    private readonly AppDbContext _db;
    private readonly HttpClient   _http;

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
        ["b5b11801-0928-43c1-af2e-2f01acbc0eca"] = "Białystok",
        ["40de6094-8646-11e6-9066-549f350fcb0c"] = "Bydgoszcz",
        ["40de1ad1-8646-11e6-9066-549f350fcb0c"] = "Prague",
        ["40e1a0af-8646-11e6-9066-549f350fcb0c"] = "Brno",
        ["40e0eb79-8646-11e6-9066-549f350fcb0c"] = "Ostrava",
        ["40de8a5a-8646-11e6-9066-549f350fcb0c"] = "Plzeň",
        ["40e2a266-8646-11e6-9066-549f350fcb0c"] = "Liberec",
        ["40de8a5a-8646-11e6-9066-549f350fcb0c"] = "Olomouc",
        ["40de6751-8646-11e6-9066-549f350fcb0c"] = "České Budějovice",
        ["40e0c29f-8646-11e6-9066-549f350fcb0c"] = "Hradec Králové",
        ["40d8f682-8646-11e6-9066-549f350fcb0c"] = "Berlin",
        ["40d91e53-8646-11e6-9066-549f350fcb0c"] = "Hamburg",
        ["40d901a5-8646-11e6-9066-549f350fcb0c"] = "Munich",
        ["40d91025-8646-11e6-9066-549f350fcb0c"] = "Koeln",
        ["40d90407-8646-11e6-9066-549f350fcb0c"] = "Frankfurt am Main",
        ["40da3d5e-8646-11e6-9066-549f350fcb0c"] = "Essen",
        ["40d90995-8646-11e6-9066-549f350fcb0c"] = "Stuttgart",
        ["40da382b-8646-11e6-9066-549f350fcb0c"] = "Dortmund",
        ["40d911c7-8646-11e6-9066-549f350fcb0c"] = "Duesseldorf",
        ["40da6e70-8646-11e6-9066-549f350fcb0c"] = "Bremen",
        ["40da4ac8-8646-11e6-9066-549f350fcb0c"] = "Hannover",
        ["40d917f9-8646-11e6-9066-549f350fcb0c"] = "Leipzig",
        ["40da79b3-8646-11e6-9066-549f350fcb0c"] = "Duisburg",
        ["40d90d0f-8646-11e6-9066-549f350fcb0c"] = "Nuernberg",
        ["40db219f-8646-11e6-9066-549f350fcb0c"] = "Dresden",
        ["40da3a44-8646-11e6-9066-549f350fcb0c"] = "Bochum",
        ["40da70aa-8646-11e6-9066-549f350fcb0c"] = "Wuppertal",
        ["40dad33a-8646-11e6-9066-549f350fcb0c"] = "Bielefeld",
        ["40dadbff-8646-11e6-9066-549f350fcb0c"] = "Bonn",
        ["40d90c3a-8646-11e6-9066-549f350fcb0c"] = "Mannheim",
        ["40d912c2-8646-11e6-9066-549f350fcb0c"] = "Karlsruhe",
        ["40dd4112-8646-11e6-9066-549f350fcb0c"] = "Wiesbaden",
        ["40dc47e2-8646-11e6-9066-549f350fcb0c"] = "Muenster",
        ["40ddc67e-8646-11e6-9066-549f350fcb0c"] = "Gelsenkirchen",
        ["40da8ddc-8646-11e6-9066-549f350fcb0c"] = "Aachen",
        ["40da838e-8646-11e6-9066-549f350fcb0c"] = "Moenchengladbach",
        ["40da3fd1-8646-11e6-9066-549f350fcb0c"] = "Augsburg",
        ["40da6fab-8646-11e6-9066-549f350fcb0c"] = "Chemnitz",
        ["40d928aa-8646-11e6-9066-549f350fcb0c"] = "Braunschweig",
        ["40dbe90d-8646-11e6-9066-549f350fcb0c"] = "Krefeld",
        ["40da5768-8646-11e6-9066-549f350fcb0c"] = "Halle",
        ["40dbe253-8646-11e6-9066-549f350fcb0c"] = "Kiel",
        ["40da54cb-8646-11e6-9066-549f350fcb0c"] = "Magdeburg",
        ["40d902e6-8646-11e6-9066-549f350fcb0c"] = "Neue Neustadt",
        ["40da7d07-8646-11e6-9066-549f350fcb0c"] = "Oberhausen",
        ["40d8ff3b-8646-11e6-9066-549f350fcb0c"] = "Freiburg",
        ["40dc0389-8646-11e6-9066-549f350fcb0c"] = "Luebeck",
        ["40db4593-8646-11e6-9066-549f350fcb0c"] = "Erfurt",
        ["40db8000-8646-11e6-9066-549f350fcb0c"] = "Hagen",
        ["40da4248-8646-11e6-9066-549f350fcb0c"] = "Rostock",
        ["40dbdcd2-8646-11e6-9066-549f350fcb0c"] = "Kassel",
        ["40dc0e9e-8646-11e6-9066-549f350fcb0c"] = "Mainz",
        ["40dcbfdd-8646-11e6-9066-549f350fcb0c"] = "Saarbruecken",
        ["40dba27d-8646-11e6-9066-549f350fcb0c"] = "Herne",
        ["40dc4560-8646-11e6-9066-549f350fcb0c"] = "Muelheim",
        ["40dc63d2-8646-11e6-9066-549f350fcb0c"] = "Osnabrueck",
        ["40da6d2c-8646-11e6-9066-549f350fcb0c"] = "Oldenburg",
        ["40dc7203-8646-11e6-9066-549f350fcb0c"] = "Potsdam",
        ["40d90575-8646-11e6-9066-549f350fcb0c"] = "Darmstadt",
        ["40d915ac-8646-11e6-9066-549f350fcb0c"] = "Wuerzburg",
        ["40dc7b76-8646-11e6-9066-549f350fcb0c"] = "Regensburg",
        ["40dd5460-8646-11e6-9066-549f350fcb0c"] = "Wolfsburg",
        ["40dbd358-8646-11e6-9066-549f350fcb0c"] = "Ingolstadt",
        ["40dd1618-8646-11e6-9066-549f350fcb0c"] = "Ulm",
        ["40da743c-8646-11e6-9066-549f350fcb0c"] = "Trier",
        ["40d92538-8646-11e6-9066-549f350fcb0c"] = "Worms",
        
    };
    private static readonly Dictionary<string, string> _stationDict = new();
    private static readonly JsonDocumentOptions _docOpt = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    public FlixbusCrawler(AppDbContext db, IHttpClientFactory factory)
    {
        _db   = db;
        _http = factory.CreateClient("flixbus");
    }

    private static Guid GuidFromUid(string uid)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(uid));
        return new Guid(bytes);
    }

    /// <summary>
    /// 生成 8 位大写 Base36 哈希，取 SHA1(uid) 前 6 字节并 padding 到 8 字节。
    /// </summary>
    private static string GenerateShortHash8(string input)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input)); // 20 字节

        // 取前 6 字节
        var part = hash.Take(6).ToArray();
        // 拷贝到 8 字节数组，剩余字节为 0
        var buf = new byte[8];
        Array.Copy(part, 0, buf, 0, part.Length);

        // 小端序转 UInt64
        var value = BitConverter.ToUInt64(buf, 0);

        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var sb = new StringBuilder();
        while (value > 0 && sb.Length < 8)
        {
            sb.Insert(0, chars[(int)(value % 36)]);
            value /= 36;
        }
        return sb.Length == 0 ? "00000000" : sb.ToString().PadLeft(8, '0');
    }

    public async Task<int> CrawlAsync(string fromCityId, string toCityId, DateTime dateUtc)
    {
        if (dateUtc == default) dateUtc = DateTime.UtcNow.Date;
        var url  = BuildFlixbusUrl(fromCityId, toCityId, dateUtc);
        var json = await GetRawAsync(url);

        using var doc = JsonDocument.Parse(json, _docOpt);
        if (!doc.RootElement.TryGetProperty("trips", out var trips) || trips.ValueKind != JsonValueKind.Array)
            return 0;

        var toInsert = new List<TransportOffer>();
        foreach (var trip in trips.EnumerateArray())
        {
            if (!trip.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var kv in results.EnumerateObject())
            {
                var obj    = kv.Value;
                if (!obj.TryGetProperty("uid", out var uidProp)) continue;
                var uidStr = uidProp.GetString();
                if (string.IsNullOrWhiteSpace(uidStr)) continue;

                var tripId  = GuidFromUid(uidStr);
                var shortId = GenerateShortHash8(uidStr);

                // 去重
                if (toInsert.Any(t => t.Id == tripId) || await _db.TransportOffers.FindAsync(tripId) is not null)
                    continue;

                if (!obj.TryGetProperty("departure", out var dep) || !obj.TryGetProperty("arrival", out var arr))
                    continue;

                var depDt = DateTime.Parse(dep.GetProperty("date").GetString()!, null, DateTimeStyles.AdjustToUniversal);
                var arrDt = DateTime.Parse(arr.GetProperty("date").GetString()!, null, DateTimeStyles.AdjustToUniversal);

                var status       = obj.TryGetProperty("status", out var st) ? st.GetString()! : string.Empty;
                var transferType = obj.TryGetProperty("transfer_type", out var tt) ? tt.GetString()! : string.Empty;
                var mot = obj.TryGetProperty("means_of_transport", out var motP) && motP.ValueKind == JsonValueKind.String
                          ? motP.GetString()! : "bus";

                // 价格
                var priceTotal  = 0m; var priceAvg = 0m; var priceOrig = 0m; var currency = "GBP";
                if (obj.TryGetProperty("price", out var price) &&
                    price.TryGetProperty("total", out var totalProp) && totalProp.TryGetDecimal(out priceTotal))
                {
                    if (price.TryGetProperty("average", out var avgP) && avgP.ValueKind == JsonValueKind.Number)
                        priceAvg = avgP.GetDecimal();
                    if (price.TryGetProperty("original", out var origP) && origP.ValueKind == JsonValueKind.Number)
                        priceOrig = origP.GetDecimal();
                    if (price.TryGetProperty("currency", out var curP) && curP.ValueKind == JsonValueKind.String)
                        currency = curP.GetString()!;
                }

                var seatsAvail = obj.TryGetProperty("available", out var avail) && avail.TryGetProperty("seats", out var sp) && sp.TryGetInt32(out var s) ? s : 0;
                var durHours   = 0; var durMinutes = 0;
                if (obj.TryGetProperty("duration", out var dur))
                {
                    dur.TryGetProperty("hours", out var hP); dur.TryGetProperty("minutes", out var mP);
                    hP.TryGetInt32(out durHours); mP.TryGetInt32(out durMinutes);
                }
                var midStops = obj.TryGetProperty("intermediate_stations_count", out var stpP) && stpP.TryGetInt32(out var c) ? c : 0;

                var depCityId = dep.GetProperty("city_id").GetString()!;
                var depStaId  = dep.GetProperty("station_id").GetString()!;
                var arrCityId = arr.GetProperty("city_id").GetString()!;
                var arrStaId  = arr.GetProperty("station_id").GetString()!;

                var depCity = GetOrAdd(_cityDict, depCityId, depCityId);
                var arrCity = GetOrAdd(_cityDict, arrCityId, arrCityId);
                var depSta  = GetOrAdd(_stationDict, depStaId,  depStaId);
                var arrSta  = GetOrAdd(_stationDict, arrStaId,  arrStaId);

                toInsert.Add(new TransportOffer
                {
                    Id                   = tripId,
                    Provider             = "FlixBus",
                    Status               = status,
                    TransferType         = transferType,
                    DepartureDate        = DateOnly.FromDateTime(depDt),
                    DepartureTime        = TimeOnly.FromDateTime(depDt),
                    ArrivalDate          = DateOnly.FromDateTime(arrDt),
                    ArrivalTime          = TimeOnly.FromDateTime(arrDt),
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
                    MeansOfTransport     = mot,
                    ShortId              = shortId
                });
            }
        }

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

    public static string BuildFlixbusUrl(string from, string to, DateTime dateUtc)
        => QueryHelpers.AddQueryString("https://global.api.flixbus.com/search/service/v4/search", new Dictionary<string, string>
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

        return body;
    }

    private static string GetOrAdd(Dictionary<string, string> dict, string key, string fallback)
    {
        if (!dict.TryGetValue(key, out var name))
            dict[key] = name = fallback;
        return name;
    }
}
