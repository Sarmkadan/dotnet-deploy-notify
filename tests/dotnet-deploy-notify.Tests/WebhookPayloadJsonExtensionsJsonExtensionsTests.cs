using System;
using System.Text.Json;
using Xunit;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Tests
{
    public class WebhookPayloadJsonExtensionsJsonExtensionsTests
    {
        private static WebhookPayload CreatePayload()
        {
            return new WebhookPayload
            {
                EventId = "evt-1",
                EventType = "deployment",
                Timestamp = new DateTime(2026, 10, 25, 10, 30, 0, DateTimeKind.Utc),
                Data = new WebhookData
                {
                    ProjectName = "Shop",
                    Version = "2.1.0",
                    Status = "Success",
                    Environment = "Production",
                    DurationSeconds = 42
                }
            };
        }

        [Fact]
        public void ToJson_Payload_ProducesCompactJsonWithCoreFields()
        {
            var json = CreatePayload().ToJson();

            Assert.DoesNotContain("\n", json);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal("evt-1", root.GetProperty("EventId").GetString());
            Assert.Equal("deployment", root.GetProperty("EventType").GetString());
            Assert.Equal("dotnet-deploy-notify", root.GetProperty("Source").GetString());
            Assert.Equal("1.0.0", root.GetProperty("SchemaVersion").GetString());
            Assert.Equal("Shop", root.GetProperty("Data").GetProperty("ProjectName").GetString());
            Assert.Equal(42, root.GetProperty("Data").GetProperty("DurationSeconds").GetInt32());
        }

        [Fact]
        public void ToJson_ThenDeserialize_RoundTripsCoreFields()
        {
            var original = CreatePayload();

            var restored = JsonSerializer.Deserialize<WebhookPayload>(original.ToJson());

            Assert.NotNull(restored);
            Assert.Equal(original.EventId, restored!.EventId);
            Assert.Equal(original.Timestamp, restored.Timestamp);
            Assert.Equal(original.Data.Version, restored.Data.Version);
            Assert.Equal(original.Data.DurationSeconds, restored.Data.DurationSeconds);
            Assert.True(restored.IsValid());
        }

        [Fact]
        public void ToJson_NullDuration_SerializesAsNullAndEmptyErrorsAsArray()
        {
            var payload = CreatePayload();
            payload.Data.DurationSeconds = null;

            using var doc = JsonDocument.Parse(payload.ToJson());
            var data = doc.RootElement.GetProperty("Data");

            Assert.Equal(JsonValueKind.Null, data.GetProperty("DurationSeconds").ValueKind);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.GetProperty("Errors").ValueKind);
            Assert.Equal(0, doc.RootElement.GetProperty("Errors").GetArrayLength());
        }

        [Fact]
        public void ToJson_ZeroDuration_IsWrittenAsZeroNotNull()
        {
            var payload = CreatePayload();
            payload.Data.DurationSeconds = 0;

            using var doc = JsonDocument.Parse(payload.ToJson());

            Assert.Equal(0, doc.RootElement.GetProperty("Data").GetProperty("DurationSeconds").GetInt32());
        }

        [Fact]
        public void ToJson_MessageWithQuotesAndUnicode_IsEscapedAndRecoverable()
        {
            var message = "Deploy \"v2\" \\ done ✅ <ok>";
            var payload = CreatePayload();
            payload.Data.Message = message;

            using var doc = JsonDocument.Parse(payload.ToJson());

            Assert.Equal(message, doc.RootElement.GetProperty("Data").GetProperty("Message").GetString());
        }

        [Fact]
        public void ToJson_CustomProperties_WritesScalarValuesWithJsonTypes()
        {
            var payload = CreatePayload();
            payload.Data.CustomProperties["retries"] = 3;
            payload.Data.CustomProperties["canary"] = true;
            payload.Data.CustomProperties["region"] = "eu-west";

            using var doc = JsonDocument.Parse(payload.ToJson());
            var props = doc.RootElement.GetProperty("Data").GetProperty("CustomProperties");

            Assert.Equal(3, props.GetProperty("retries").GetInt32());
            Assert.True(props.GetProperty("canary").GetBoolean());
            Assert.Equal("eu-west", props.GetProperty("region").GetString());
        }

        [Fact]
        public void ToJson_UnserializableCustomProperty_ThrowsNotSupportedException()
        {
            var payload = CreatePayload();
            payload.Data.CustomProperties["callback"] = new Func<int>(() => 1);

            Assert.Throws<NotSupportedException>(() => payload.ToJson());
        }

        [Fact]
        public void ToJson_NullPayload_ThrowsNullReferenceException()
        {
            WebhookPayload? payload = null;

            Assert.Throws<NullReferenceException>(() => payload!.ToJson());
        }
    }
}
