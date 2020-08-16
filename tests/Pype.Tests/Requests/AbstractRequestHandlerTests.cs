using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using Pype.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Tests.Requests
{
    public class AbstractRequestHandlerTests
    {
        [Fact]
        public async Task HandleAsync_calls_abstract_internal_method_and_returns_data()
        {
            IRequestHandler<Request, string> sut = new RequestHandler();

            var request = new Request();

            var result = await sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match(data => data, error => null).Should().NotBeNull().And.Be("Handle invoked");
        }

        [Fact]
        public async Task HandleAsync_calls_abstract_internal_method_and_returns_unit()
        {
            StringWriter writer = new StringWriter();

            IRequestHandler<UnitRequest> sut = new UnitRequestHandler(writer);

            var request = new UnitRequest();

            var result = await sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match(unit => 1, error => 2).Should().Be(1);
            writer.ToString().Should().Be("Handle invoked");
        }
    }
}
