# CacheEntryExtensions

Provides a set of static extension methods for querying the expiration state of a cache entry. These methods allow callers to retrieve the remaining time-to-live, check validity, compute the entry’s age, and determine how close it is to expiration as a percentage. They are designed to work with any cache entry type `T` that exposes the underlying expiration metadata.

## API

### `GetTimeToLive<T>(this T entry)`

Returns the remaining time until the cache entry expires.

- **Parameters**  
  `entry` – The cache entry to query.  
  `T` – The type of the cache entry.

- **Returns**  
  A `TimeSpan` representing the time remaining before the entry expires. If the entry has already expired, the returned value is `TimeSpan.Zero`.

- **Throws**  
  `ArgumentNullException` – if `entry` is `null`.

---

### `IsValid<T>(this T entry)`

Indicates whether the cache entry is still valid (i.e., has not expired).

- **Parameters**  
  `entry` – The cache entry to check.  
  `T` – The type of the cache entry.

- **Returns**  
  `true` if the entry has not yet expired; otherwise, `false`.

- **Throws**  
  `ArgumentNullException` – if `entry` is `null`.

---

### `GetAge<T>(this T entry)`

Returns the amount of time that has elapsed since the cache entry was created or last refreshed.

- **Parameters**  
  `entry` – The cache entry to query.  
  `T` – The type of the cache entry.

- **Returns**  
  A `TimeSpan` representing the age of the entry. For entries that have already expired, the age may exceed the original time-to-live.

- **Throws**  
  `ArgumentNullException` – if `entry` is `null`.

---

### `GetExpirationPercentage<T>(this T entry)`

Returns a value between 0.0 and 1.0 (inclusive) indicating how close the entry is to expiration, where 0.0 means just created and 1.0 means fully expired.

- **Parameters**  
  `entry` – The cache entry to evaluate.  
  `T` – The type of the cache entry.

- **Returns**  
  A `double` representing the expiration progress. A value of 1.0 indicates the entry has expired; values greater than 1.0 are possible if the entry has been expired for longer than its original TTL.

- **Throws**  
  `ArgumentNullException` – if `entry` is `null`.  
  `InvalidOperationException` – if the entry does not have a defined time-to-live (e.g., sliding expiration with no absolute expiration).

---

## Usage

### Example 1: Checking validity and remaining TTL

```csharp
using dotnet_deploy_notify; // Namespace assumed for CacheEntryExtensions

public void LogCacheEntryStatus(ICacheEntry<MyData> entry)
{
    if (entry == null) throw new ArgumentNullException(nameof(entry));

    bool valid = entry.IsValid();
    TimeSpan ttl = entry.GetTimeToLive();

    Console.WriteLine($"Entry valid: {valid}, TTL: {ttl.TotalSeconds:F1}s");
}
```

### Example 2: Proactive refresh based on expiration percentage

```csharp
public async Task<MyData> GetOrRefreshAsync(ICacheEntry<MyData> entry, Func<Task<MyData>> refreshFactory)
{
    // If the entry is more than 80% expired, refresh it asynchronously.
    if (entry.GetExpirationPercentage() > 0.8)
    {
        var newData = await refreshFactory();
        // Replace the entry with fresh data (implementation omitted).
        return newData;
    }

    return entry.Value; // Assumes entry exposes a Value property.
}
```

## Notes

- **Edge cases**  
  - For entries that have already expired, `GetTimeToLive` returns `TimeSpan.Zero`, and `IsValid` returns `false`.  
  - `GetAge` may return a value larger than the original TTL if the entry has been expired for some time.  
  - `GetExpirationPercentage` can exceed 1.0 when the entry has been expired longer than its configured TTL.  
  - If the cache entry does not have a defined absolute or sliding expiration, `GetExpirationPercentage` throws `InvalidOperationException`.

- **Thread safety**  
  All methods are read-only and do not modify the cache entry. They are safe to call concurrently from multiple threads without additional synchronization, provided the underlying cache entry implementation itself is thread-safe for read operations.
