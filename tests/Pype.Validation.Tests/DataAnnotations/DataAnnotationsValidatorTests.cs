using FluentAssertions;
using Pype.Requests;
using Pype.Validation.Abstractions;
using Pype.Validation.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Validation.Tests.DataAnnotations
{
    public class DataAnnotationsValidatorTests
    {
        public class Request : IRequest
        {
            [Required]
            public string Prop { get; set; }

            [Range(0, 2)]
            public int Prop2 { get; set; }

            [Required, Object]
            public Inner Prop3 { get; set; }
        }

        public class Inner
        { 
            [Required]
            public string Prop { get; set; }
        }


        [Fact]
        public async Task ValidateAsync_succeeds_when_object_is_valid()
        {
            var sut = new DataAnnotationsValidator<Request>();

            var inner = new Inner { Prop = "inner" };
            var request = new Request { Prop = "test", Prop2 = 1, Prop3 = inner };

            var result = await sut.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match<bool?>(success => success, e => default).Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_fails_when_object_is_not_valid()
        {
            var sut = new DataAnnotationsValidator<Request>();

            var request = new Request { Prop = null };

            var result = await sut.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match<bool?>(v => default, e => e is ValidationError).Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_validates_all_validation_attributes()
        {
            var sut = new DataAnnotationsValidator<Request>();

            var request = new Request { Prop = "test", Prop2 = 99 };

            var result = await sut.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match<bool?>(v => default, e => e is ValidationError).Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAsync_validates_complex_properties()
        {
            var sut = new DataAnnotationsValidator<Request>();

            var invalidInner = new Inner { Prop = null };
            var request = new Request { Prop = "test", Prop2 = 99, Prop3 = invalidInner };

            var result = await sut.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match<bool?>(v => default, e => e is ValidationError).Should().BeTrue();
        }
    }
}
