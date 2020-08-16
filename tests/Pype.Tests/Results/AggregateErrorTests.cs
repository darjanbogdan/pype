using FluentAssertions;
using Pype.Results;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Pype.Tests.Results
{
    public class AggregateErrorTests
    {
        [Fact]
        public void Constructor_throws_when_message_is_null()
        {
            Action createError = () => new AggregateError(errors: null);

            createError.Should().Throw<ArgumentNullException>().WithMessage("*errors*");
        }

        [Fact]
        public void Constructor_maps_arguments()
        {
            var errors = new List<Error>();

            var aggregate = new AggregateError(errors);

            aggregate.Errors.Should().NotBeNull();
            aggregate.Errors.Equals(errors);
            aggregate.Code.Should().Be(default(int?));
            aggregate.Data.Should().Be(default);
            aggregate.Message.Should().NotBeNullOrEmpty();
        }
    }
}
