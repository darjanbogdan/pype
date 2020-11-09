using FluentAssertions;
using Pype.Background.Channels.Processing;
using System;
using Xunit;

namespace Pype.Background.Channels.Tests.Processing
{
    public class BackgroundHandleProxyProcessorConfigurationTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(Int32.MaxValue)]
        public void Constructor_throws_when_max_concurrency_is_valid(int concurrency)
        {
            Action createError = () => new BackgroundHandleProxyProcessorConfiguration(concurrency);

            createError.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_throws_when_max_concurrency_is_invalid(int invalidMaxConcurrency)
        {
            Action createError = () => new BackgroundHandleProxyProcessorConfiguration(invalidMaxConcurrency);

            createError.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*MaxConcurrency*");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(Int32.MaxValue)]
        public void MaxConcurrency_initialization_when_max_concurrency_is_valid(int concurrency)
        {
            Action createError = () => new BackgroundHandleProxyProcessorConfiguration { MaxConcurrency = concurrency };

            createError.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MaxConcurrency_initialization_throws_when_max_concurrency_is_invalid(int invalidMaxConcurrency)
        {
            Action createError = () => new BackgroundHandleProxyProcessorConfiguration
            { 
                MaxConcurrency = invalidMaxConcurrency
            };

            createError.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*MaxConcurrency*");
        }
    }
}
