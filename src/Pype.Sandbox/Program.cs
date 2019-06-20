using Pype.Notifications;
using Pype.Requests;
using Pype.Sandbox.Users;
using Pype.Validation;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace Pype.Sandbox
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var asm = typeof(Program).Assembly;
            var valAsm = typeof(ValidationException).Assembly;

            var container = new Container();

            container.Register(typeof(IRequestHandler<,>), new[] { asm });
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(RequestHandlerValidationDecorator<,>));

            container.Collection.Register(typeof(INotificationHandler<>), new[] { asm });

            container.Register<IBus>(() => new Bus(container.GetInstance));

            container.Collection.Register(typeof(IValidator<>), new[] { asm });

            container.Verify();

            var bus = container.GetInstance<IBus>();

            var result = await bus.SendAsync(new CreateUserRequest());

            await bus.PublishAsync(new UserCreatedNotification());
            await bus.PublishAsync(new UserExtendedCreatedNotification());
        }

    }
}
