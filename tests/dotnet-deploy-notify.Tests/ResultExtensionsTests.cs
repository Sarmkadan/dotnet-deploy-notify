using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetDeployNotify.Results;
using Xunit;
using Moq;

namespace DotNetDeployNotify.Tests
{
    public class ResultExtensionsTests
    {
        [Fact]
        public async Task Test_TryAsync_ValidFunc_ReturnsSuccess()
        {
            // Arrange
            Func<Task<string>> func = async () =>
            {
                return await Task.FromResult("Hello World");
            };

            // Act
            var result = await ResultExtensions.TryAsync(func);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Hello World", result.Value);
        }

        [Fact]
        public async Task Test_TryAsync_InvalidFunc_ThrowsException()
        {
            // Arrange
            Func<Task<string>> func = async () =>
            {
                throw new Exception("Test");
            };

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(async () => await ResultExtensions.TryAsync(func));
        }

        [Fact]
        public void Test_Try_ValidFunc_ReturnsSuccess()
        {
            // Arrange
            Func<string> func = () =>
            {
                return "Hello World";
            };

            // Act
            var result = ResultExtensions.Try(func);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Hello World", result.Value);
        }

        [Fact]
        public void Test_Try_InvalidFunc_ThrowsException()
        {
            // Arrange
            Func<string> func = () =>
            {
                throw new Exception("Test");
            };

            // Act and Assert
            Assert.Throws<Exception>(() => ResultExtensions.Try(func));
        }

        [Fact]
        public void Test_Combine_NoResults_ReturnsSuccess()
        {
            // Act
            var result = ResultExtensions.Combine(Enumerable.Empty<Result<string>>());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Test_Combine_MultipleResults_ReturnsSuccess()
        {
            // Arrange
            var results = new List<Result<string>>
            {
                Result<string>.Ok("Result1"),
                Result<string>.Ok("Result2")
            };

            // Act
            var result = ResultExtensions.Combine(results);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "Result1", "Result2" }, result.Value);
        }

        [Fact]
        public void Test_Combine_MultipleResults_WithErrors_ReturnsFailure()
        {
            // Arrange
            var results = new List<Result<string>>
            {
                Result<string>.Ok("Result1"),
                Result<string>.Fail("Error1"),
                Result<string>.Ok("Result2")
            };

            // Act
            var result = ResultExtensions.Combine(results);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(new[] { "Error1" }, result.Errors);
        }
    }
}