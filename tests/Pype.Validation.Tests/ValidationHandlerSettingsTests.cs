using FluentAssertions;
using Xunit;

namespace Pype.Validation.Tests
{
    public class ValidationHandlerSettingsTests
    {
        [Fact]
        public void StopOnFailure_default_value_is_true()
        {
            var settings = new ValidationHandlerSettings();

            settings.StopOnFailure.Should().BeTrue();
        }
    }
}
