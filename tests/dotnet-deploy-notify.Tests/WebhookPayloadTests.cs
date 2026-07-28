using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Tests
{
    public class WebhookPayloadTests
    {
        [Fact]
        public void TestDefaultConstructor()
        {
            // Arrange
            var webhookPayload = new WebhookPayload();

            // Act

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(webhookPayload.EventId));
            Assert.False(string.IsNullOrWhiteSpace(webhookPayload.EventType));
            Assert.NotNull(webhookPayload.Data);
            Assert.False(string.IsNullOrWhiteSpace(webhookPayload.Source));
            Assert.False(string.IsNullOrWhiteSpace(webhookPayload.SchemaVersion));
        }

        [Fact]
        public void TestIsValid()
        {
            // Arrange
            var webhookPayload = new WebhookPayload
            {
                EventId = "12345",
                EventType = "deployment",
                Data = new WebhookData
                {
                    ProjectName = "MyProject",
                    Version = "1.0.0",
                    Status = "success"
                }
            };

            // Act

            // Assert
            Assert.True(webhookPayload.IsValid());
        }

        [Fact]
        public void TestIsValidNullEventId()
        {
            // Arrange
            var webhookPayload = new WebhookPayload
            {
                EventId = null,
                EventType = "deployment",
                Data = new WebhookData
                {
                    ProjectName = "MyProject",
                    Version = "1.0.0",
                    Status = "success"
                }
            };

            // Act

            // Assert
            Assert.False(webhookPayload.IsValid());
        }

        [Fact]
        public void TestIsValidNullEventType()
        {
            // Arrange
            var webhookPayload = new WebhookPayload
            {
                EventId = "12345",
                EventType = null,
                Data = new WebhookData
                {
                    ProjectName = "MyProject",
                    Version = "1.0.0",
                    Status = "success"
                }
            };

            // Act

            // Assert
            Assert.False(webhookPayload.IsValid());
        }

        [Fact]
        public void TestIsValidNullData()
        {
            // Arrange
            var webhookPayload = new WebhookPayload
            {
                EventId = "12345",
                EventType = "deployment",
                Data = null
            };

            // Act

            // Assert
            Assert.False(webhookPayload.IsValid());
        }
    }
}