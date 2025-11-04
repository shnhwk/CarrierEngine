using System.Collections;
using System.Net;

namespace CarrierEngine.Infrastructure.Http;

/// <summary>
/// Provides static helpers for combining URL segments and merging query parameters.
/// <para>
/// Examples:
/// <code>
/// // Basic combination
/// Url.Combine("https://api.example.com", "v1", "users");
/// // → https://api.example.com/v1/users
///
/// // Merge new query parameters into existing URL
/// Url.Combine("https://api.example.com/v1/users?active=true",
///             new { page = 2, sort = "asc" });
/// // → https://api.example.com/v1/users?active=true&amp;page=2&amp;sort=asc
///
/// // Dictionary and arrays become repeated query keys
/// var dict = new Dictionary&lt;string, object?&gt;
/// {
///     ["q"] = "bass fishing",
///     ["limit"] = 25,
///     ["tags"] = new[] { "bass", "lake" }
/// };
/// Url.Combine("http://localhost:5001", "api", "search", dict);
/// // → http://localhost:5001/api/search?q=bass%20fishing&amp;limit=25&amp;tags=bass&amp;tags=lake
///
/// // Fluent helpers
/// "http://localhost:5001/api/users"
///     .WithQuery(new { page = 3 })
///     .WithQueryParam("q", "wacky rig");
/// // → http://localhost:5001/api/users?page=3&amp;q=wacky%20rig
/// </code>
/// </para>
/// </summary>
public static class Url
{
    /// <summary>
    /// Combines URL path segments into a single URL without query merging.
    /// </summary>
    /// <param name="baseOrFirst">The first or base part of the URL (may include scheme).</param>
    /// <param name="moreParts">Additional path segments to append.</param>
    /// <returns>A well-formed combined URL.</returns>
    public static string Combine(string baseOrFirst, params string[] moreParts)
        => Combine(baseOrFirst, query: null, moreParts);

    /// <summary>
    /// Combines URL path segments and merges optional query parameters.
    /// </summary>
    /// <param name="baseOrFirst">The first or base part of the URL (may include scheme).</param>
    /// <param name="query">An object or dictionary representing query parameters to merge.</param>
    /// <param name="moreParts">Additional path segments to append.</param>
    /// <returns>A well-formed combined URL including merged query parameters.</returns>
    private static string Combine(string baseOrFirst, object? query, params string[] moreParts)
    {
        var allParts = new[] { baseOrFirst }.Concat(moreParts).ToArray();

        // Separate out any existing query from path segments
        var partsNoQuery = new List<string>(allParts.Length);
        var existingQueryPairs = new List<KeyValuePair<string, string>>();

        foreach (var p in allParts)
        {
            if (string.IsNullOrWhiteSpace(p)) continue;
            var s = p.Trim();

            var qIdx = s.IndexOf('?');
            if (qIdx >= 0)
            {
                var queryStr = s[(qIdx + 1)..];
                s = s[..qIdx];
                ParseQuery(queryStr, existingQueryPairs);
            }

            if (!string.IsNullOrWhiteSpace(s)) partsNoQuery.Add(s);
        }

        // Normalize path and scheme
        var pathParts = partsNoQuery
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim('/'))
            .ToList();

        if (pathParts.Count == 0)
            return BuildUrl("", existingQueryPairs);

        var scheme = "";
        var first = pathParts[0];
        var schemeIdx = first.IndexOf("://", StringComparison.Ordinal);
        if (schemeIdx > 0)
        {
            scheme = first[..(schemeIdx + 3)];
            pathParts[0] = first[(schemeIdx + 3)..].Trim('/');
        }

        var core = scheme + string.Join("/", pathParts);

        // Merge extra query
        if (query is not null)
        {
            var added = ToQueryPairs(query);
            existingQueryPairs.AddRange(added);
        }

        return BuildUrl(core, existingQueryPairs);
    }

    /// <summary>
    /// Adds or merges query parameters onto an existing URL.
    /// </summary>
    /// <param name="url">The base URL (may already contain a query).</param>
    /// <param name="query">An object or dictionary representing query parameters to merge.</param>
    /// <returns>A new URL string containing merged query parameters.</returns>
    public static string WithQuery(this string url, object query)
        => Combine(url, query);

    /// <summary>
    /// Adds or merges a single query parameter onto an existing URL.
    /// </summary>
    /// <param name="url">The base URL.</param>
    /// <param name="key">The query parameter key.</param>
    /// <param name="value">The query parameter value (converted to string).</param>
    /// <returns>A new URL string containing the added query parameter.</returns>
    public static string WithQueryParam(this string url, string key, object? value)
        => Combine(url, new[] { new KeyValuePair<string, object?>(key, value) });

    // ---------------- internal helpers ----------------

    private static string BuildUrl(string core, List<KeyValuePair<string, string>> query)
    {
        if (query.Count == 0) return core;

        var q = string.Join("&", query.Select(kvp =>
            $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        return core + "?" + q;
    }

    private static void ParseQuery(string query, List<KeyValuePair<string, string>> into)
    {
        foreach (var kv in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = kv.Split('=', 2);
            var key = WebUtility.UrlDecode(pair[0]);
            var val = pair.Length > 1 ? WebUtility.UrlDecode(pair[1]) : "";
            if (!string.IsNullOrEmpty(key))
                into.Add(new KeyValuePair<string, string>(key, val));
        }
    }

    private static List<KeyValuePair<string, string>> ToQueryPairs(object obj)
    {
        var pairs = new List<KeyValuePair<string, string>>();

        switch (obj)
        {
            case IEnumerable<KeyValuePair<string, object?>> kvs:
                pairs.AddRange(from kv in kvs
                    from v in ExpandValue(kv.Value)
                    select new KeyValuePair<string, string>(kv.Key, v));
                break;

            case IDictionary dict:
                pairs.AddRange(from DictionaryEntry de in dict
                    let key = de.Key.ToString() ?? ""
                    from v in ExpandValue(de.Value)
                    select new KeyValuePair<string, string>(key, v));
                break;

            default:
                var props = obj.GetType().GetProperties();
                pairs.AddRange(from p in props
                    let key = p.Name
                    let value = p.GetValue(obj)
                    from v in ExpandValue(value)
                    select new KeyValuePair<string, string>(key, v));
                break;
        }

        return pairs;
    }

    private static List<string> ExpandValue(object? value)
    {
        var result = new List<string>();

        switch (value)
        {
            case null:
                result.Add(""); return result;
            case string s:
                result.Add(s); return result;
            case byte[] bytes:
                result.Add(Convert.ToBase64String(bytes)); return result;
            case DateTime dt:
                result.Add(dt.ToUniversalTime().ToString("o")); return result;
            case DateTimeOffset dto:
                result.Add(dto.ToUniversalTime().ToString("o")); return result;
            case IEnumerable e and not string:
            {
                foreach (var item in e)
                    result.AddRange(ExpandValue(item));
                return result;
            }
            default:
                result.Add(value.ToString() ?? "");
                return result;
        }
    }
}