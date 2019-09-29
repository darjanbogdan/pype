using Enexure.MicroBus;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pype.Benchmarks.Bus
{
    public class MicroBusDependencyScope : MicroBusDependencyResolver, IDependencyScope
    {
        private readonly Scope _scope;
        private readonly Container _container;

        public MicroBusDependencyScope(Container container)
            : base(container)
        {
            _container = container;
            _scope = AsyncScopedLifestyle.BeginScope(container);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public object GetService(Type serviceType)
            => _container.GetInstance(serviceType);

        public T GetService<T>()
            => (T)_container.GetInstance(typeof(T));

        public IEnumerable<object> GetServices(Type serviceType)
            => _container.GetAllInstances(serviceType);

        public IEnumerable<T> GetServices<T>()
            => (IEnumerable<T>)_container.GetAllInstances(typeof(IEnumerable<T>));
    }
}
