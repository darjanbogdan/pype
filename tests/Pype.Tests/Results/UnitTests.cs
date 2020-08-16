using FluentAssertions;
using System;
using Xunit;

namespace Pype.Tests.Results
{
    public class UnitTests
    {
        [Fact]
        public void Unit_objects_are_treated_equally()
        {
            var unit1 = new Unit();
            var unit2 = new Unit();

            Object.ReferenceEquals(unit1, unit2).Should().BeFalse();
            unit1.Should().Be(unit2);
            unit1.Equals(unit2).Should().BeTrue();
            (unit1 == unit2).Should().BeTrue();
        }

        [Fact]
        public void Unit_objects_have_the_same_hash_code()
        {
            var unit1 = new Unit();
            var unit2 = new Unit();

            unit1.GetHashCode().Should().Be(default);
            unit2.GetHashCode().Should().Be(default);
        }

        [Fact]
        public void Instance_property_returns_different_instances()
        {
            var unit1 = Unit.Instance;
            var unit2 = Unit.Instance;

            unit1.Should().NotBeSameAs(unit2);
        }
    }
}
