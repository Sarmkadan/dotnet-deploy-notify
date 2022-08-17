# CollectionExtensions

A set of extension methods that add common collection‑manipulation utilities to the base .NET collection types. The methods are pure functions (except where they mutate the supplied collection) and are intended to simplify everyday tasks such as conditional addition, bulk removal, chunking, distinct selection, partitioning, random access, and string formatting.

## API

### AddIfNotExists<T>
**Purpose** – Adds an item to a collection only when an equal item is not already present. Equality is determined by the default `Equals` implementation for `T`.  
**Parameters**  
- `collection`: `ICollection<T>` – the target collection.  
- `item`: `T` – the item to add.  
**Return value** – `void`.  
**Throws** – `ArgumentNullException` if `collection` is `null`.

### AddRange<T>
**Purpose** – Appends all elements of the supplied enumerable to the collection.  
**Parameters**  
- `collection`: `ICollection<T>` – the target collection.  
- `items`: `IEnumerable<T>` – the elements to add.  
**Return value** – `void`.  
**Throws** – `ArgumentNullException` if `collection` or `items` is `null`.

### RemoveWhere<T>
**Purpose** – Removes all elements that satisfy the supplied predicate and returns the number of elements removed.  
**Parameters**  
- `collection`: `ICollection<T>` – the collection to modify.  
- `match`: `Predicate<T>` – a delegate that returns `true` for elements to be removed.  
**Return value** – `int` – the count of removed elements.  
**Throws** – `ArgumentNullException` if `collection` or `match` is `null`.

### Chunk<T>
**Purpose** – Splits a sequence into successive chunks of a fixed size. The final chunk may contain fewer than `size` elements if the source does not divide evenly.  
**Parameters**  
- `source`: `IEnumerable<T>` – the sequence to chunk.  
- `size`: `int` – the maximum number of elements per chunk; must be greater than zero.  
**Return value** – `IEnumerable<List<T>>` – each inner list represents a chunk.  
**Throws** –  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `size` is less than or equal to zero.

### DistinctBy<T, TKey>
**Purpose** – Returns distinct elements from a sequence based on a key selector; the first element encountered for each distinct key is yielded.  
**Parameters**  
- `source`: `IEnumerable<T>` – the source sequence.  
- `keySelector`: `Func<T, TKey>` – a function that extracts the key used for comparison.  
**Return value** – `IEnumerable<T>` – a sequence containing the distinct elements.  
**Throws** – `ArgumentNullException` if `source` or `keySelector` is `null`.

### Partition<T>
**Purpose** – Divides a sequence into two lists according to a predicate: one list contains elements for which the predicate returns `true`, the other contains the remaining elements.  
**Parameters**  
- `source`: `IEnumerable<T>` – the sequence to partition.  
- `predicate`: `Func<T, bool>` – a function that classifies each element.  
**Return value** – `(List<T> True, List<T> False)` – a value tuple where `True` holds elements satisfying the predicate and `False` holds the rest.  
**Throws** – `ArgumentNullException` if `source` or `predicate` is `null`.

### GetAtIndexOrDefault<T>
**Purpose** – Retrieves the element at a specific index, returning the default value for `T` when the index is out of bounds.  
**Parameters**  
- `list`: `IList<T>` – the list to index into.  
- `index`: `int` – the zero‑based position to retrieve.  
**Return value** – `T?` – the element at `index` or `default(T)` if `index < 0` or `index >= list.Count`.  
**Throws** – `ArgumentNullException` if `list` is `null`.

### IsNullOrEmpty<T>
**Purpose** – Checks whether a collection is `null` or contains no elements.  
**Parameters**  
- `collection`: `ICollection<T>` – the collection to test.  
**Return value** – `bool` – `true` if `collection` is `null` or its `Count` is `0`; otherwise `false`.  
**Throws** – none (the method safely handles a `null` argument).

### HasItems<T>
**Purpose** – Determines whether a collection is not `null` and contains at least one element.  
**Parameters**  
- `collection`: `ICollection<T>` – the collection to test.  
**Return value** – `bool` – `true` if `collection != null` and `collection.Count > 0`; otherwise `false`.  
**Throws** – none.

### ToCommaSeparatedString<T>
**Purpose** – Produces a single string where each element is converted to its string representation and separated by commas.  
**Parameters**  
- `source`: `IEnumerable<T>` – the sequence to format.  
**Return value** – `string` – a comma‑separated list; returns an empty string if `source` is empty.  
**Throws** – `ArgumentNullException` if `source` is `null`.

### GetRandom<T>
**Purpose** – Returns a randomly selected element from the list.  
**Parameters**  
- `list`: `IList<T>` – the source list.  
**Return value** – `T?` – a random element, or `default(T)` if the list is `null` or empty.  
**Throws** – `ArgumentNullException` if `list` is `null`.

### Shuffle<T>
**Purpose** – Randomly reorders the elements of the list in place using the Fisher‑Yates algorithm.  
**Parameters**  
- `list`: `IList<T>` – the list to shuffle.  
**Return value** – `void`.  
**Throws** – `ArgumentNullException` if `list` is `null`.

### CountBy<T, TKey>
**Purpose** – Counts how many times each key appears in a sequence, as projected by a key selector.  
**Parameters**  
- `source`: `IEnumerable<T>` – the input sequence.  
- `keySelector`: `Func<T, TKey>` – function that extracts the key for each element.  
**Return value** – `Dictionary<TKey, int>` – maps each distinct key to its occurrence count.  
**Throws** – `ArgumentNullException` if `source` or `keySelector` is `null`.

### Flatten<T>
**Purpose** – Flattens one level of nesting in a sequence of sequences, producing a single sequence that contains all inner elements in their original order.  
**Parameters**  
- `source`: `IEnumerable<IEnumerable<T>>` – the nested sequence to flatten.  
**Return value** – `IEnumerable<T>` – a sequence of all elements from the inner sequences.  
**Throws** – `ArgumentNullException` if `source` is `null`.

## Usage

### Example 1 – Conditional addition and bulk removal
```csharp
using System.Collections.Generic;

var numbers = new List<int> { 1, 2, 3, 4, 5 };

// Add 6 only if it is not already present
numbers.AddIfNotExists(6);          // numbers now: 1,2,3,4,5,6

// Try to add a duplicate; nothing changes
numbers.AddIfNotExists(6);          // numbers unchanged

// Remove all even numbers
int removed = numbers.RemoveWhere(n => n % 2 == 0);
// removed == 3 (2,4,6 were removed)
// numbers now: 1,3,5

// Quick checks
bool hasItems = numbers.HasItems(); // true
bool empty    = numbers.IsNullOrEmpty(); // false
```

### Example 2 – Chunking, distinct selection, and partitioning
```csharp
using System.Collections.Generic;
using System.Linq;

var words = new List<string>
{
    "apple", "banana", "apricot", "blueberry", "avocado", "banana", "cherry"
};

// Get distinct words by their first letter
var distinctByFirst = words.DistinctBy(w => w[0]).ToList();
// distinctByFirst: apple, banana, cherry

// Split the list into chunks of size 3
var chunks = words.Chunk(3).ToList();
// chunks: [apple, banana, apricot], [blueberry, avocado, banana], [cherry]

// Partition the original list into words that start with 'a' and the rest
var (startsWithA, others) = words.Partition(w => w[0] == 'a');
// startsWithA: apple, apricot, avocado
// others: banana, blueberry, banana, cherry
```

## Notes
- All methods that accept a collection or sequence argument will throw `ArgumentNullException` when that argument is `null`, unless explicitly noted otherwise (e.g., `IsNullOrEmpty` and `HasItems` safely handle `null`).
- The methods do not maintain any internal state; they are thread‑safe with respect to their own execution. However, if the supplied collection is modified concurrently by another thread, the behavior becomes undefined and may result in exceptions or inconsistent results. Callers must provide external synchronization when concurrent access is possible.
- `Chunk` produces a new `List<T>` for each chunk; enumerating the result multiple times will recreate those lists each time.
- `DistinctBy` and `Partition` buffer elements as needed to evaluate the predicate or key selector; they are not suitable for infinite sequences unless the caller knows the sequence will terminate.
- `GetRandom` uses the default `System.Random` instance internally; repeated calls in quick succession may produce the same seed‑based sequence if not wrapped in a shared `Random` object. For cryptographic quality is not guaranteed.
- `Shuffle` mutates the supplied list; if the original ordering is needed elsewhere, work on a copy (`list.ToList().Shuffle()`).
- `Flatten` performs a single‑level flattening; nested structures deeper than one level remain nested. For recursive flattening, apply the method repeatedly or implement a custom recursive solution.
