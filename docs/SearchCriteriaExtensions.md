# SearchCriteriaExtensions

Provides a set of static extension methods for building and modifying `SearchCriteria` objects and for filtering collections of `DeploymentNotification` instances. The methods are pure functions that return new instances without mutating their inputs, making them suitable for fluent‑style queries and safe to use from multiple threads.

## API

### Combine
**Purpose** – Returns a new `SearchCriteria` that represents the logical conjunction of two criteria.  
**Parameters**  
- `left` – The first `SearchCriteria` to combine.  
- `right` – The second `SearchCriteria` to combine.  
**Return Value** – A `SearchCriteria` that encapsulates the combined filter expressions of `left` and `right`.  
**Exceptions** – Throws `ArgumentNullException` if either `left` or `right` is `null`.

### ClearFilters
**Purpose** – Returns a new `SearchCriteria` with all filter conditions removed, leaving only any pagination or sorting settings that may be present.  
**Parameters**  
- `criteria` – The `SearchCriteria` to clear.  
**Return Value** – A `SearchCriteria` instance that contains no filter predicates.  
**Exceptions** – Throws `ArgumentNullException` if `criteria` is `null`.

### WithPagination
**Purpose** – Returns a new `SearchCriteria` configured with pagination information.  
**Parameters**  
- `criteria` – The `SearchCriteria` to augment with pagination.  
- `pageSize` – The maximum number of items to return per page (must be ≥ 1).  
- `pageNumber` – The 1‑based index of the page to retrieve (must be ≥ 1; defaults to 1 if omitted).  
**Return Value** – A `SearchCriteria` that includes the specified pagination settings.  
**Exceptions** –  
- Throws `ArgumentNullException` if `criteria` is `null`.  
- Throws `ArgumentOutOfRangeException` if `pageSize` or `pageNumber` is less than 1.

### FilterByPriority
**Purpose** – Filters a sequence of `DeploymentNotification` objects, yielding only those whose priority matches the specified value.  
**Parameters**  
- `source` – The sequence of `DeploymentNotification` instances to filter.  
- `priority` – The priority level to match (defined by the `Priority` enumeration).  
**Return Value** – An `IEnumerable<DeploymentNotification>` containing the elements from `source` that have the requested priority.  
**Exceptions** – Throws `ArgumentNullException` if `source` is `null`.

## Usage

```csharp
// Build a criteria that combines a base filter with pagination.
var baseCrit = SearchCriteria.ClearFilters(); // start with a clean criteria
var combined = SearchCriteria.Combine(baseCrit, SearchCriteria.WithPagination(baseCrit, 20, 2));
var results = notificationRepo.Search(combined);
```

```csharp
// Retrieve only high‑priority notifications from a repository.
IEnumerable<DeploymentNotification> all = notificationRepo.GetAll();
IEnumerable<DeploymentNotification> highPri = all.FilterByPriority(Priority.High);
foreach (var n in highPri)
{
    Console.WriteLine($"{n.Id}: {n.Message}");
}
```

## Notes
- All extension methods are immutable; they never modify the instance passed in and always return a new object (or a new enumerable).  
- Because they do not rely on mutable state, the methods are thread‑safe and can be invoked concurrently from multiple threads.  
- Passing `null` for any argument that expects a reference type will result in an `ArgumentNullException`; this is the only condition under which the methods throw.  
- The `WithPagination` method treats page numbers as 1‑based; supplying a value of zero or negative will trigger an `ArgumentOutOfRangeException`.  
- If the underlying `SearchCriteria` implementation imposes additional constraints (e.g., mutually exclusive filters), those rules are enforced by the returned object and may cause later operations to fail; the extension methods themselves do not validate such semantic rules.
