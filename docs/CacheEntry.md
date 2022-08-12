# CacheEntry

A generic cache entry that stores a value with metadata for tracking expiration, access patterns, and cache statistics. It provides methods for retrieving, setting, and managing cached data with support for composite key building and repository-level operations.

## API

### `public T? Value`

The cached value. Returns `null` if the entry has expired or has not been set.

### `public DateTime ExpiresAt`

The absolute expiration time for this cache entry. After this time, the entry is considered stale and will not be returned by retrieval methods.

### `public DateTime CreatedAt`

The timestamp when this cache entry was created.

### `public int TotalItems`

The total number of items currently stored in this cache entry. Used to track size or batch operations.

### `public long Hits`

The total number of successful retrievals (`Get<T>`) from this cache entry.

### `public long Misses`

The total number of failed retrievals (`Get<T>`) from this cache entry, typically due to expiration or absence.

### `public DateTime LastCleanup`

The timestamp of the last cache cleanup operation affecting this entry.

### `public MemoryCacheService`

Reference to the underlying memory cache service managing this entry.

### `public T? Get<T>()`

Retrieves the cached value of type `T`. Returns `null` if the entry is expired or not found.

- **Type Parameters**: `T` – The type of the cached value.
- **Returns**: The cached value, or `null` if not available.
- **Throws**: May throw if the underlying cache service encounters an error during retrieval.

### `public void Set<T>(T value)`

Stores a value in the cache with the current entry’s expiration policy.

- **Type Parameters**: `T` – The type of the value to cache.
- **Parameters**: `value` – The value to store.
- **Throws**: May throw if the cache service is unavailable or if the value exceeds size limits.

### `public void Remove()`

Removes this cache entry from the underlying cache store immediately.

### `public void Clear()`

Removes all cached data associated with this entry across all keys.

### `public CacheStatistics GetStatistics()`

Returns a snapshot of cache statistics for this entry, including hits, misses, and size.

- **Returns**: A `CacheStatistics` object containing usage metrics.

### `public CachedRepository`

Gets the repository associated with this cache entry, enabling bulk operations.

### `public async Task<List<T>> GetAllAsync<T>()`

Asynchronously retrieves all cached values of type `T` associated with this entry.

- **Type Parameters**: `T` – The type of the cached values.
- **Returns**: A list of cached values; may be empty if none exist.
- **Throws**: May throw if the underlying store is inaccessible or if deserialization fails.

### `public void InvalidateCache()`

Marks the cache entry as invalid, triggering cleanup on next access or scheduled maintenance.

### `public CacheKeyBuilder Add(string keyPart)`

Adds a key component to the composite key builder for this entry.

- **Parameters**: `keyPart` – A segment of the composite key.
- **Returns**: The updated `CacheKeyBuilder` for method chaining.

### `public CacheKeyBuilder Add(int keyPart)`

Adds an integer key component to the composite key builder.

- **Parameters**: `keyPart` – An integer segment of the composite key.
- **Returns**: The updated `CacheKeyBuilder` for method chaining.

### `public CacheKeyBuilder Add(Guid keyPart)`

Adds a GUID key component to the composite key builder.

- **Parameters**: `keyPart` – A GUID segment of the composite key.
- **Returns**: The updated `CacheKeyBuilder` for method chaining.

### `public string Build()`

Finalizes the composite key from the added components.

- **Returns**: A string representing the fully constructed cache key.

## Usage
