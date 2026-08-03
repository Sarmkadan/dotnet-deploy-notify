## CacheKeyBuilderTests

The `CacheKeyBuilderTests` class contains tests for the `CacheKeyBuilder` class.

These tests verify that the `CacheKeyBuilder` class behaves as expected.

Example usage:
```csharp
public void DeterministicOutput_ForSameInputs_ShouldReturnSameKey
public void DifferentInputs_ShouldProduceDifferentKeys
public void NullAndEmptySegments_ShouldBeIgnored
public void SegmentOrdering_ShouldAffectKey
public void AddObject_ShouldHandleNullAndNonNullValues
```
