using FluentAssertions;
using Pype.Validation.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace Pype.Validation.Tests.DataAnnotations
{
    public class AggregateValidationResultTests
    {
        [Fact]
        public void Constructor_throws_when_aggregate_name_is_null()
        {
            Action createError = () => new AggregateValidationResult(aggregateName: null, validationResults: null);

            createError.Should().Throw<ArgumentNullException>().WithMessage("*aggregateName*");
        }

        [Fact]
        public void Constructor_throws_when_validation_results_are_null()
        {
            Action createError = () => new AggregateValidationResult(aggregateName: String.Empty, validationResults: null);

            createError.Should().Throw<ArgumentNullException>().WithMessage("*validationResults*");
        }

        [Fact]
        public void Constructor_maps_arguments()
        {
            var results = new List<ValidationResult>();
            var aggregateResult = new AggregateValidationResult("New aggregate", results);

            aggregateResult.AggregateName.Should().Be("New aggregate");
            aggregateResult.MemberNames.Should().HaveCount(1);
            aggregateResult.Results.Equals(results);
        }
    }
}
