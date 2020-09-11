using Pype.Notifications;
using Pype.Requests;
using Pype.Sandbox.Users;
using Pype.Validation;
using Pype.Validation.Abstractions;
using Pype.Validation.DataAnnotations;
using Pype.Validation.FluentValidation;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pype.Sandbox
{
    class Program
    {
//        async Task Main2(string[] args)
//{
//    Assembly[] assemblies = GetApplicationAssemblies();

//    // basic setup
//    var container = new Container();
//    container.Register(typeof(IRequestHandler<,>), assemblies);
//    container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));

//    // data validation
//    var dataAnnotationValidator = typeof(DataAnnotationValidator<>);
//    var fluentValidatorTypes = container.GetTypesToRegister(typeof(AbstractFluentValidator<>), assemblies);
//    var customValidatorTypes = container.GetTypesToRegister(typeof(IValidator<>), assemblies);
//    var orderedValidatorTypes = new[] { dataAnnotationValidator }.Union(fluentValidatorTypes).Union(customValidatorTypes);

//    container.Collection.Register(typeof(IValidator<>), orderedValidatorTypes);
//    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));

//    // composition verification
//    container.Verify();

//    // usage
//    var createUserCommand = new CreateUserCommand() { Name = "Foo", Email = "bar@baz.com" };

//    var handler = container.GetInstance<IRequestHandler<CreateUserCommand, Unit>>();
//    var result = await handler.HandleAsync(createUserCommand);
//    bool successful = result.Match(_ => true, error => false);

//    // or

//    var bus = container.GetInstance<IBus>();
//    var result = await bus.SendAsync(createUserCommand);
//    bool successful = result.Match(_ => true, error => false);
//}
        
        async static Task Main()
        {
            var asm = typeof(Program).Assembly;

            var pype = CreatePype(asm);

            var command = new CreateUserCommand
            {
                Name = "DS",
                Age = 12,
                Email = "dsad@gai.com",
                Password = "1231234"
            };

            var result = await pype.SendAsync(command);

            //await pype.PublishAsync(new UserCreatedNotification());
            //await pype.PublishAsync(new UserExtendedCreatedNotification());
        }

        //private static IBus CreatePype2(params Assembly[] assemblies)
        //{
        //    // instantiates SimpleInjector container
        //    var container = new Container();

        //    // registeres all the types in assemblies that implement given open generic handler interface
        //    container.Register(typeof(IRequestHandler<,>), assemblies);

        //    // registers bus which serve as request handler mediator
        //    container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));

        //    // Validation related
        //    Type dataAnnotationValidator = typeof(DataAnnotationValidator<>);

        //    // gets all types in assemblies which inherit from open generic class 
        //    IEnumerable<Type> fluentValidatorTypes = container.GetTypesToRegister(typeof(AbstractFluentValidator<>), assemblies);

        //    // get all types in assemblies which implement open generic interface
        //    IEnumerable<Type> customValidatorTypes = container.GetTypesToRegister(typeof(IValidator<>), assemblies);

        //    // define order of validators: data annotations -> fluent validators -> custom validators
        //    IEnumerable<Type> orderedValidatorTypes = new [] { dataAnnotationValidator }.Union(fluentValidatorTypes).Union(customValidatorTypes);

        //    // registeres ordered open generic validator types which implement open generic interface
        //    container.Collection.Register(typeof(IValidator<>), orderedValidators);

        //    // registeres open generic validation decorator around open generic handler interface
        //    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));
        //}
        
        private static IBus CreatePype(params Assembly[] assemblies)
        {
            var container = new Container();

            container.Register(typeof(IRequestHandler<,>), assemblies);

            container.Collection.Register(typeof(INotificationHandler<>), assemblies);

            container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));
            
            var dataAnnotationValidator = typeof(DataAnnotationsValidator<>);

            var fluentValidators = container.GetTypesToRegister(typeof(AbstractFluentValidator<>), assemblies);
            
            var validators = container.GetTypesToRegister(typeof(IValidator<>), assemblies);
            var customValidators = GetGenericInterfaceImplementationTypes(assemblies, typeof(IValidator<>));

            var orderedValidators = new List<Type> { dataAnnotationValidator }.Concat(fluentValidators).Concat(customValidators);

            container.Collection.Register(typeof(IValidator<>), orderedValidators);

            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(ValidationHandler<,>));

            container.Verify();

            return container.GetInstance<IBus>();
        }

        /// <summary>
        /// Gets all implementation types from assembly collection using open generic interface type.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="openGenericType">Type of the open generic.</param>
        /// <returns></returns>
        public static Type[] GetGenericInterfaceImplementationTypes(Assembly[] assemblies, Type openGenericInterfaceType)
        {
            return assemblies.SelectMany(asm => GetGenericInterfaceImplementationTypes(asm, openGenericInterfaceType)).ToArray();
        }

        /// <summary>
        /// Gets all implementation types from assembly using open generic interface type.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="openGenericType">Type of the open generic.</param>
        /// <returns></returns>
        public static Type[] GetGenericInterfaceImplementationTypes(Assembly assembly, Type openGenericInterfaceType)
        {
            return assembly.GetTypes()
                .Where(t =>
                    !t.IsAbstract
                    && !t.IsInterface
                    && t.GetInterfaces()
                        .Any(i =>
                            i.GetTypeInfo().IsGenericType
                            && i.GetGenericTypeDefinition() == openGenericInterfaceType
                        )
                    )
                .ToArray();
        }
    }
}
