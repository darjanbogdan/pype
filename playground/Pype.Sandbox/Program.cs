#define CHANNELS // QUEUE, CHANNELS, DATAFLOW, REACTIVE

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if QUEUE
using Pype.Background.Queue;
using Pype.Background.Queue.Processing;
#elif CHANNELS
using System.Threading.Channels;
using Pype.Background.Abstractions;
using Pype.Background.Channels;
using Pype.Background.Channels.Processing;
#elif DATAFLOW
using System.Threading.Tasks.Dataflow;
using Pype.Background.DataFlow;
using Pype.Background.DataFlow.Processing;
#elif REACTIVE
using System.Reactive.Subjects;
using Pype.Background.Abstractions;
using Pype.Background.Reactive.Processing;
#endif
using Pype.Notifications;
using Pype.Requests;
using Pype.Sandbox.Users;
using Pype.Validation;
using Pype.Validation.Abstractions;
using Pype.Validation.DataAnnotations;
using Pype.Validation.FluentValidation;
using SimpleInjector;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox
{
    public class Program
    {
        public static async Task Main()
        {
            var container = new Container();

            IHost host = new HostBuilder()
                .ConfigureLogging(opts => opts.AddConsole())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddSimpleInjector(container, options =>
                    {
                        options.AddHostedService<BackgroundHandleProxyProcessor>();
                        options.AddHostedService<NotificationProducer>();

                        options.AddLogging();
                    });
                })
                .UseConsoleLifetime()
                .Build()
                .UseSimpleInjector(container);

            RegisterPype(container, new[] { typeof(Program).Assembly });

            container.Verify();

            await host.RunAsync();
        }

        private static void RegisterPype(Container container, Assembly[] assemblies)
        {
            // Bus
            container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));

            // Request handler pipeline
            container.Register(typeof(IRequestHandler<,>), assemblies);

            // validation
            var dataAnnotationValidator = typeof(DataAnnotationsValidator<>);
            var fluentValidatorTypes = container.GetTypesToRegister(typeof(AbstractFluentValidator<>), assemblies);
            var customValidatorTypes = container.GetTypesToRegister(typeof(IValidator<>), assemblies);
            var orderedValidatorTypes = new[] { dataAnnotationValidator }.Union(fluentValidatorTypes).Union(customValidatorTypes);
            container.Collection.Register(typeof(IValidator<>), orderedValidatorTypes);

            container.RegisterInstance(new ValidationHandlerSettings());
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(ValidationHandler<,>));

            //Notification handler pipeline
            container.Collection.Register(typeof(INotificationHandler<>), assemblies);

            // background processing
#if QUEUE
            container.RegisterSingleton<IBackgroundHandleProxyQueue, BackgroundHandleProxyQueue>();
            container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));
#elif CHANNELS
            container.RegisterSingleton(() => Channel.CreateUnbounded<HandleProxy>());
            container.RegisterSingleton(() => container.GetInstance<Channel<HandleProxy>>().Reader);
            container.RegisterSingleton(() => container.GetInstance<Channel<HandleProxy>>().Writer);

            container.RegisterInstance(new BackgroundHandleProxyProcessorConfiguration(maxConcurrency: 1));
            container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));
#elif DATAFLOW
            container.RegisterSingleton(() => new HandleProxyActionBlock(new ExecutionDataflowBlockOptions()));
            container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));
#elif REACTIVE
            container.RegisterSingleton(() => new Subject<HandleProxy>());
            container.RegisterDecorator(typeof(INotificationHandler<>), typeof(Background.Reactive.BackgroundNotificationHandler<>));
#endif
        }

        public class NotificationProducer : BackgroundService
        {
            private readonly IBus _bus;
            private readonly Container _container;

            public NotificationProducer(IBus bus, Container container)
            {
                _bus = bus;
                _container = container;
            }
            
            // note: different producing technique can mimick different loads. Example, continuous, regular batches, irregular spikes, high-low intervals...)
            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                //return Task.WhenAll(
                _ = Enumerable.Range(0, 10000).Select(i => _bus.PublishAsync(new UserCreatedNotification(i))).ToList();
                //);

                //for (int i = 0; i < 10000; i++)
                //{
                //    _ = _bus.PublishAsync(new UserCreatedNotification(i))
                //}

                return Task.CompletedTask;
            }
        }
    }
}
