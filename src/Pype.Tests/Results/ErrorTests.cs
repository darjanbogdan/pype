using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Text;
using Xunit;

namespace Pype.Tests.Results
{
    public class ErrorTests
    {
        [Fact]
        public void Constructor_throws_when_message_is_null()
        {
            Action createError = () => new Error(message: null);

            createError.Should().Throw<ArgumentNullException>().WithMessage("*message*");
        }

        [Fact]
        public void Constructor_does_not_throw_when_data_is_null()
        {
            Action createError = () => new Error(message: String.Empty, data: null);

            createError.Should().NotThrow();
        }

        [Fact]
        public void Constructor_does_not_throw_when_code_is_null()
        {
            Action createError = () => new Error(message: String.Empty, code: null);

            createError.Should().NotThrow();
        }

        [Fact]
        public void Constructor_maps_arguments()
        {
            var error = new Error(message: "input", data: 0, code: 1);

            error.Message.Should().Be("input");
            error.Data.Should().Be(0);
            error.Code.Should().Be(1);
        }
    }
}
