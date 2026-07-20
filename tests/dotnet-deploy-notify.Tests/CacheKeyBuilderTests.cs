using System;
using Xunit;
using DotNetDeployNotify.Caching;

namespace DotNetDeployNotify.Tests
{
    public class CacheKeyBuilderTests
    {
        [Fact]
        public void DeterministicOutput_ForSameInputs_ShouldReturnSameKey()
        {
            var builder1 = new CacheKeyBuilder()
                .Add("foo")
                .Add("bar")
                .Add(42);

            var builder2 = new CacheKeyBuilder()
                .Add("foo")
                .Add("bar")
                .Add(42);

            var key1 = builder1.ToString();
            var key2 = builder2.ToString();

            Assert.Equal(key1, key2);
        }

        [Fact]
        public void DifferentInputs_ShouldProduceDifferentKeys()
        {
            var builder1 = new CacheKeyBuilder()
                .Add("foo")
                .Add("bar");

            var builder2 = new CacheKeyBuilder()
                .Add("foo")
                .Add("baz");

            var key1 = builder1.ToString();
            var key2 = builder2.ToString();

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void NullAndEmptySegments_ShouldBeIgnored()
        {
            var builderWithNulls = new CacheKeyBuilder()
                .Add((string?)null)
                .Add(string.Empty)
                .Add("foo");

            var builderWithoutNulls = new CacheKeyBuilder()
                .Add("foo");

            var keyWithNulls = builderWithNulls.ToString();
            var keyWithoutNulls = builderWithoutNulls.ToString();

            Assert.Equal(keyWithNulls, keyWithoutNulls);
        }

        [Fact]
        public void SegmentOrdering_ShouldAffectKey()
        {
            var builder1 = new CacheKeyBuilder()
                .Add("foo")
                .Add("bar");

            var builder2 = new CacheKeyBuilder()
                .Add("bar")
                .Add("foo");

            var key1 = builder1.ToString();
            var key2 = builder2.ToString();

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void AddObject_ShouldHandleNullAndNonNullValues()
        {
            var builderWithNullObject = new CacheKeyBuilder()
                .Add((object?)null)
                .Add("foo");

            var builderWithNonNullObject = new CacheKeyBuilder()
                .Add(123)
                .Add("foo");

            var keyWithNullObject = builderWithNullObject.ToString();
            var keyWithNonNullObject = builderWithNonNullObject.ToString();

            // Null object should be ignored, so keyWithNullObject should equal keyWithoutNullObject
            var builderWithoutNullObject = new CacheKeyBuilder()
                .Add("foo");

            var keyWithoutNullObject = builderWithoutNullObject.ToString();

            Assert.Equal(keyWithNullObject, keyWithoutNullObject);
            Assert.NotEqual(keyWithNullObject, keyWithNonNullObject);
        }
    }
}
