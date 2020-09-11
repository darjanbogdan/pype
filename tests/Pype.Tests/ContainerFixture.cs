using Pype.Notifications;
using Pype.Requests;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.IO;

namespace Pype.Tests
{
    public class ContainerFixture
    {
        public ContainerFixture()
        {
            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var assemblies = new[] { typeof(ContainerFixture).Assembly };

            Container.Register(typeof(IRequestHandler<,>), assemblies);
            Container.Collection.Register(typeof(INotificationHandler<>), assemblies);
            
            Container.RegisterSingleton<IBus>(() => new Bus(Container.GetInstance));
            Container.Register(() => new StringWriter(), Lifestyle.Scoped);

            Container.Verify();
        }

        public Container Container { get; } = new Container();
    }
}
