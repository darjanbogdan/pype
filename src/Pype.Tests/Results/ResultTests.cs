using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Pype.Tests.Results
{
    public class ResultTests
    {
        [Fact]
        public void Constructor_throws_when_data_is_null()
        {
            Action createResult = () => Result.Ok<string>(data: null);

            createResult.Should().Throw<ArgumentNullException>().WithMessage("*data*");
        }

        [Fact]
        public void Constructor_throws_when_error_is_null()
        {
            Action createResult = () => Result.Fail<string>(error: null);

            createResult.Should().Throw<ArgumentNullException>().WithMessage("*error*");
        }

        [Fact]
        public void Match_throws_when_success_delegate_is_null()
        {
            var result = Result.Ok();

            Action matchResult = () => result.Match<bool>(success: null, default);

            matchResult.Should().Throw<ArgumentNullException>().WithMessage("*success*");
        }

        [Fact]
        public void Match_calls_success_delegate_when_result_is_ok ()
        {
            var result = Result.Ok();

            bool value = result.Match(data => true, error => false);

            value.Should().BeTrue();
        }

        [Fact]
        public void Match_throws_when_error_delegate_is_null()
        {
            var result = Result.Fail<string>(new Error(String.Empty));

            Action matchResult = () => result.Match<bool>(default, error: null);

            matchResult.Should().Throw<ArgumentNullException>().WithMessage("*error*");
        }

        [Fact]
        public void Match_calls_success_delegate_when_result_is_fail()
        {
            var result = Result.Fail<string>(new Error(String.Empty));

            bool value = result.Match(data => true, error => false);

            value.Should().BeFalse();
        }
    }
}
