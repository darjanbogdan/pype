using Pype.Notifications;
using Pype.Requests;
using Pype.Sandbox.Users;
using SimpleInjector;
using System.Reflection;
using System.Threading.Tasks;

namespace Pype.Sandbox
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var asm = typeof(Program).Assembly;

            var pype = CreatePype(asm);

            await pype.SendAsync(new CreateUserCommand());

            await pype.PublishAsync(new UserCreatedNotification());
            await pype.PublishAsync(new UserExtendedCreatedNotification());
        }

        private static IBus CreatePype(params Assembly[] assemblies)
        {
            var container = new Container();

            container.Register(typeof(IRequestHandler<,>), assemblies);

            container.Collection.Register(typeof(INotificationHandler<>), assemblies);

            container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));

            container.Verify();

            return container.GetInstance<IBus>();
        }
    }
}
