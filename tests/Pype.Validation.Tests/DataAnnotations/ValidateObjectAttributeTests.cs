using FluentAssertions;
using Pype.Validation.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace Pype.Validation.Tests.DataAnnotations
{
    public class ValidateObjectAttributeTests
    {
        [Fact]
        public void IsValid_succeeds_when_value_is_null()
        {
            var attribute = new ObjectAttribute();

            var result = attribute.IsValid(value: null);

            result.Should().BeTrue();
        }
    }
}
