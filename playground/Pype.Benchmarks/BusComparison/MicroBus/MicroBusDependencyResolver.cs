using Enexure.MicroBus;
using SimpleInjector;

namespace Pype.Benchmarks.BusComparison
{
    public class MicroBusDependencyResolver : IDependencyResolver
    {
        private readonly Container _container;

        public MicroBusDependencyResolver(Container container)
        {
            _container = container;
        }

        public IDependencyScope BeginScope()
            => new MicroBusDependencyScope(_container);
    }
}
