using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StokeeFishing.Services;

/// <summary>
/// Service for fetching Grand Exchange prices from the RS Wiki API.
/// Uses the WeirdGloop bulk API for efficient price lookups.
/// </summary>
public class GrandExchangeService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<int, GEItemPrice> _priceCache = new();
    private DateTime _lastBulkFetch = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);

    private const string BulkApiUrl = "https://chisel.weirdgloop.org/gazproj/gazbot/rs_dump.json";
    private const string ItemDetailUrl = "https://services.runescape.com/m=itemdb_rs/api/catalogue/detail.json?item=";

    public GrandExchangeService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
    }

    /// <summary>
    /// GE price information for an item.
    /// </summary>
    public record GEItemPrice(
        int ItemId,
        string Name,
        long Price,
        DateTime FetchedAt)
    {
        public bool IsStale => DateTime.UtcNow - FetchedAt > TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Get the cached price for an item, or null if not cached.
    /// </summary>
    public GEItemPrice? GetCachedPrice(int itemId)
    {
        if (_priceCache.TryGetValue(itemId, out var price) && !price.IsStale)
            return price;
        return null;
    }

    /// <summary>
    /// Fetch all GE prices in bulk. This is the most efficient way to get prices.
    /// </summary>
    public async Task<bool> FetchBulkPricesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(BulkApiUrl, ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response);

            if (data == null) return false;

            _priceCache.Clear();
            var now = DateTime.UtcNow;

            foreach (var (key, value) in data)
            {
                // Skip metadata keys
                if (key.StartsWith("%")) continue;

                if (int.TryParse(key, out var itemId) && value.ValueKind == JsonValueKind.Object)
                {
                    var name = value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
                    var price = value.TryGetProperty("price", out var priceProp) ? priceProp.GetInt64() : 0;

                    _priceCache[itemId] = new GEItemPrice(itemId, name, price, now);
                }
            }

            _lastBulkFetch = now;
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to fetch bulk GE prices: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the price for a single item. Uses cache if available, otherwise fetches.
    /// </summary>
    public async Task<GEItemPrice?> GetPriceAsync(int itemId, CancellationToken ct = default)
    {
        // Check cache first
        if (GetCachedPrice(itemId) is { } cached)
            return cached;

        // If bulk cache is stale, refresh it
        if (DateTime.UtcNow - _lastBulkFetch > _cacheExpiry)
        {
            await FetchBulkPricesAsync(ct);
            if (GetCachedPrice(itemId) is { } bulkCached)
                return bulkCached;
        }

        // Fall back to individual item lookup
        try
        {
            var response = await _httpClient.GetStringAsync($"{ItemDetailUrl}{itemId}", ct);
            var data = JsonSerializer.Deserialize<ItemDetailResponse>(response);

            if (data?.Item != null)
            {
                var price = ParsePrice(data.Item.Current?.Price ?? "0");
                var result = new GEItemPrice(itemId, data.Item.Name ?? "", price, DateTime.UtcNow);
                _priceCache[itemId] = result;
                return result;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to fetch price for item {itemId}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Get prices for multiple items at once.
    /// </summary>
    public async Task<Dictionary<int, GEItemPrice>> GetPricesAsync(IEnumerable<int> itemIds, CancellationToken ct = default)
    {
        var result = new Dictionary<int, GEItemPrice>();

        // Ensure bulk cache is fresh
        if (DateTime.UtcNow - _lastBulkFetch > _cacheExpiry)
            await FetchBulkPricesAsync(ct);

        foreach (var id in itemIds)
        {
            if (GetCachedPrice(id) is { } cached)
                result[id] = cached;
        }

        return result;
    }

    /// <summary>
    /// Calculate the total value of items.
    /// </summary>
    public long CalculateTotalValue(IEnumerable<(int itemId, long quantity)> items)
    {
        long total = 0;
        foreach (var (itemId, quantity) in items)
        {
            if (GetCachedPrice(itemId) is { } price)
                total += price.Price * quantity;
        }
        return total;
    }

    private static long ParsePrice(string priceStr)
    {
        if (string.IsNullOrWhiteSpace(priceStr))
            return 0;

        // Handle suffixes like "1.2k", "5.5m", "1.2b"
        priceStr = priceStr.Trim().ToLowerInvariant().Replace(",", "");

        double multiplier = 1;
        if (priceStr.EndsWith('k'))
        {
            multiplier = 1_000;
            priceStr = priceStr[..^1];
        }
        else if (priceStr.EndsWith('m'))
        {
            multiplier = 1_000_000;
            priceStr = priceStr[..^1];
        }
        else if (priceStr.EndsWith('b'))
        {
            multiplier = 1_000_000_000;
            priceStr = priceStr[..^1];
        }

        if (double.TryParse(priceStr, out var value))
            return (long)(value * multiplier);

        return 0;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    // JSON response models for the Jagex API
    private record ItemDetailResponse(
        [property: JsonPropertyName("item")] ItemDetail? Item);

    private record ItemDetail(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("current")] PriceInfo? Current);

    private record PriceInfo(
        [property: JsonPropertyName("price")] string? Price);
}

/// <summary>
/// Static accessor for the GE service (singleton pattern for scripts).
/// </summary>
public static class GrandExchange
{
    private static GrandExchangeService? _instance;
    private static readonly object _lock = new();

    public static GrandExchangeService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new GrandExchangeService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Get price synchronously (blocking). Use sparingly.
    /// </summary>
    public static long GetPrice(int itemId)
    {
        var cached = Instance.GetCachedPrice(itemId);
        if (cached != null)
            return cached.Price;

        // Try to fetch synchronously
        try
        {
            var task = Instance.GetPriceAsync(itemId);
            task.Wait();
            return task.Result?.Price ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Pre-fetch prices for common fishing items.
    /// </summary>
    public static async Task PreloadFishPricesAsync(CancellationToken ct = default)
    {
        await Instance.FetchBulkPricesAsync(ct);
    }
}
