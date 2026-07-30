using System;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Serialization;
using Xunit;

namespace dotnet_deploy_notify.Tests
{
    public class BuildStatusConverterExtensionsTests
    {
        private readonly BuildStatusConverter _converter = new();

        #region IsSuccessful / IsFailed / IsInProgress

        [Theory]
        [InlineData(BuildStatus.Success, true)]
        [InlineData(BuildStatus.SuccessWithWarnings, true)]
        [InlineData(BuildStatus.DeploymentSuccess, true)]
        [InlineData(BuildStatus.Failed, false)]
        [InlineData(BuildStatus.Started, false)]
        public void IsSuccessful_ReturnsExpected(BuildStatus status, bool expected)
        {
            bool result = _converter.IsSuccessful(status);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(BuildStatus.Failed, true)]
        [InlineData(BuildStatus.DeploymentFailed, true)]
        [InlineData(BuildStatus.Cancelled, true)]
        [InlineData(BuildStatus.Success, false)]
        [InlineData(BuildStatus.InProgress, false)]
        public void IsFailed_ReturnsExpected(BuildStatus status, bool expected)
        {
            bool result = _converter.IsFailed(status);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(BuildStatus.Started, true)]
        [InlineData(BuildStatus.InProgress, true)]
        [InlineData(BuildStatus.Deploying, true)]
        [InlineData(BuildStatus.Success, false)]
        [InlineData(BuildStatus.Failed, false)]
        public void IsInProgress_ReturnsExpected(BuildStatus status, bool expected)
        {
            bool result = _converter.IsInProgress(status);
            Assert.Equal(expected, result);
        }

        #endregion

        #region GetDisplayName

        [Theory]
        [InlineData(BuildStatus.Started, "Build Started")]
        [InlineData(BuildStatus.InProgress, "Build In Progress")]
        [InlineData(BuildStatus.Success, "Build Success")]
        [InlineData(BuildStatus.Failed, "Build Failed")]
        [InlineData(BuildStatus.Cancelled, "Build Cancelled")]
        [InlineData(BuildStatus.SuccessWithWarnings, "Build Success (with warnings)")]
        [InlineData(BuildStatus.Deploying, "Deploying")]
        [InlineData(BuildStatus.DeploymentSuccess, "Deployment Success")]
        [InlineData(BuildStatus.DeploymentFailed, "Deployment Failed")]
        public void GetDisplayName_ReturnsExpected(BuildStatus status, string expected)
        {
            string result = _converter.GetDisplayName(status);
            Assert.Equal(expected, result);
        }

        #endregion

        #region ParseStatus / TryParseStatus

        [Theory]
        [InlineData("Started", BuildStatus.Started)]
        [InlineData("started", BuildStatus.Started)]
        [InlineData("SUCCESS", BuildStatus.Success)]
        [InlineData("SuccessWithWarnings", BuildStatus.SuccessWithWarnings)]
        public void ParseStatus_ValidString_ReturnsEnum(string input, BuildStatus expected)
        {
            BuildStatus result = _converter.ParseStatus(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseStatus_NullOrEmpty_ThrowsArgumentException(string input)
        {
            Assert.Throws<ArgumentException>(() => _converter.ParseStatus(input));
        }

        [Theory]
        [InlineData("Started", true, BuildStatus.Started)]
        [InlineData("failed", true, BuildStatus.Failed)]
        [InlineData("InvalidStatus", false, BuildStatus.Started)] // default value is Started
        [InlineData(null, false, BuildStatus.Started)]
        [InlineData("", false, BuildStatus.Started)]
        [InlineData("   ", false, BuildStatus.Started)]
        public void TryParseStatus_BehavesAsExpected(string input, bool expectedSuccess, BuildStatus expectedStatus)
        {
            bool success = _converter.TryParseStatus(input, out BuildStatus result);
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedStatus, result);
        }

        #endregion

        #region GetPriority

        [Theory]
        [InlineData(BuildStatus.Failed, NotificationPriority.Critical)]
        [InlineData(BuildStatus.DeploymentFailed, NotificationPriority.Critical)]
        [InlineData(BuildStatus.Cancelled, NotificationPriority.Critical)]
        [InlineData(BuildStatus.Deploying, NotificationPriority.High)]
        [InlineData(BuildStatus.InProgress, NotificationPriority.Normal)]
        [InlineData(BuildStatus.Started, NotificationPriority.Low)]
        [InlineData(BuildStatus.Success, NotificationPriority.Low)]
        [InlineData(BuildStatus.SuccessWithWarnings, NotificationPriority.Low)]
        [InlineData(BuildStatus.DeploymentSuccess, NotificationPriority.Low)]
        public void GetPriority_ReturnsExpected(BuildStatus status, NotificationPriority expected)
        {
            NotificationPriority result = _converter.GetPriority(status);
            Assert.Equal(expected, result);
        }

        #endregion

        #region GetCssClass

        [Theory]
        [InlineData(BuildStatus.Success, "status-success")]
        [InlineData(BuildStatus.SuccessWithWarnings, "status-success")]
        [InlineData(BuildStatus.DeploymentSuccess, "status-success")]
        [InlineData(BuildStatus.Failed, "status-failed")]
        [InlineData(BuildStatus.DeploymentFailed, "status-failed")]
        [InlineData(BuildStatus.Cancelled, "status-cancelled")]
        [InlineData(BuildStatus.InProgress, "status-in-progress")]
        [InlineData(BuildStatus.Deploying, "status-deploying")]
        [InlineData(BuildStatus.Started, "status-started")]
        [InlineData((BuildStatus)999, "status-unknown")]
        public void GetCssClass_ReturnsExpected(BuildStatus status, string expected)
        {
            string result = _converter.GetCssClass(status);
            Assert.Equal(expected, result);
        }

        #endregion

        #region IsSameAs

        [Fact]
        public void IsSameAs_ReturnsTrueWhenEqual()
        {
            bool result = _converter.IsSameAs(BuildStatus.Failed, BuildStatus.Failed);
            Assert.True(result);
        }

        [Fact]
        public void IsSameAs_ReturnsFalseWhenNotEqual()
        {
            bool result = _converter.IsSameAs(BuildStatus.Failed, BuildStatus.Success);
            Assert.False(result);
        }

        #endregion

        #region GetSeverity

        [Theory]
        [InlineData(BuildStatus.Cancelled, 10)]
        [InlineData(BuildStatus.Failed, 9)]
        [InlineData(BuildStatus.DeploymentFailed, 9)]
        [InlineData(BuildStatus.Deploying, 8)]
        [InlineData(BuildStatus.InProgress, 7)]
        [InlineData(BuildStatus.Started, 6)]
        [InlineData(BuildStatus.SuccessWithWarnings, 5)]
        [InlineData(BuildStatus.DeploymentSuccess, 4)]
        [InlineData(BuildStatus.Success, 3)]
        [InlineData((BuildStatus)999, 0)]
        public void GetSeverity_ReturnsExpected(BuildStatus status, int expected)
        {
            int result = _converter.GetSeverity(status);
            Assert.Equal(expected, result);
        }

        #endregion
    }
}
