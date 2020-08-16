using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pype.Validation.Abstractions.Tests
{
    public class ValidationErrorTests
    {
        [Fact]
        public void Constructor_throws_when_message_is_null()
        {
            Action createError = () => new ValidationError(message: null);

            createError.Should().Throw<ArgumentNullException>().WithMessage("*message*");
        }

        [Fact]
        public void Constructor_does_not_throw_when_data_is_null()
        {
            Action createError = () => new ValidationError(data: null);

            createError.Should().NotThrow();
        }

        [Fact]
        public void Constructor_initializes_properties_with_default_values()
        {
            var error = new ValidationError();

            error.Code.Should().Be(ValidationError.DefaultCode);
            error.Message.Should().Be(ValidationError.DefaultMessage);
            error.Data.Should().BeNull();
        }

        [Fact]
        public void Constructor_maps_arguments()
        {
            var error = new ValidationError(message: "New message", code: 40_000);

            error.Code.Should().Be(40_000);
            error.Message.Should().Be("New message");
            error.Data.Should().BeNull();
        }

        [Fact]
        public void Constructor_maps_arguments_with_data()
        {
            var data = new Dictionary<string, object>() { ["error"] = "example" };

            var error = new ValidationError(data, message: "New message", code: 40_000);

            error.Code.Should().Be(40_000);
            error.Message.Should().Be("New message");
            error.Data.Should().NotBeNull().And.HaveCount(1);
            error.Data.Equals(data);
        }
    }
}
