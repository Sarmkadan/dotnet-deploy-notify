using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetDeployNotify.Context;
using DotNetDeployNotify.Tests;

namespace DotNetDeployNotify.Tests
{
    public class RequestContextTests
    {
        [Fact]
        public void HappyPath_CorrelationId_ReturnsGuid()
        {
            // Arrange
            var context = new RequestContext();

            // Act
            var correlationId = context.CorrelationId;

            // Assert
            Assert.NotNull(correlationId);
            Assert.True(Guid.TryParse(correlationId, out _), "CorrelationId should be a GUID string");
        }

        [Fact]
        public void HappyPath_RequestId_ReturnsGuid()
        {
            // Arrange
            var context = new RequestContext();

            // Act
            var requestId = context.RequestId;

            // Assert
            Assert.NotNull(requestId);
            Assert.True(Guid.TryParse(requestId, out _), "RequestId should be a GUID string");
        }

        [Fact]
        public void HappyPath_RequestTime_ReturnsUtcNow()
        {
            // Arrange
            var context = new RequestContext();

            // Act
            var requestTime = context.RequestTime;

            // Assert
            Assert.NotNull(requestTime);
            Assert.True(requestTime.Kind == DateTimeKind.Utc);
        }

        [Fact]
        public void EdgeCase_NullUserId_DoesNotThrow()
        {
            // Arrange
            var context = new RequestContext();
            context.UserId = null;

            // Act and Assert
            Assert.Null(context.UserId);
        }

        [Fact]
        public void EdgeCase_EmptyClientId_DoesNotThrow()
        {
            // Arrange
            var context = new RequestContext();
            context.ClientId = null;

            // Act and Assert
            Assert.Null(context.ClientId);
        }

        [Fact]
        public void ErrorPath_SetMetadata_ThrowsOnNullKey()
        {
            // Arrange
            var context = new RequestContext();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => context.SetMetadata(null!, "value"));
        }
    }
}