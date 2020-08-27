# Pype

_To get the overview what Pype is trying to tackle and model, please check the article [series](https://dev.to/darjanbogdan/command-query-domain-introduction-5eo2) which is the basis for this implementation._
<hr>

Yet another model representation of CQS principle with helpful additions. Unlike similar solutions, this implementation is a bit more opinionated to enforce certain rules to _steer_ developers into the right direction. 

The model is simplified and reduced to decrease redundancy which happens when dealing with two set of different interfaces. Each representing command or query. In this case request can represent both, under certain discipline and behavior.

## Request and Handler
Evolves around two interfaces:

_Request_ - an object which carries information and seeks for the response:

```
/// Defines a request with response
public interface IRequest<out TResponse>
{
}
```

_RequestHandler_ - an object which handles the _Request_ and produces the response:
```
/// Defines a handler for request with response
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// Handles a request
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellation = default);
}
```

Simple implementation example:
```
public class CreateUserCommand : IRequest<User> 
{ 
    public string UserName { get; set; }
    public string Email { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly IUserStore _userStore;

    public CreateUserCommandHandler(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public Task<Result<User>> HandleAsync(CreateUserCommand command, CancellationToken cancellation)
        => _userStore.CreatUser(command.UserName, command.Email, cancellation);
}
```

## Result, Error and Unit

Besides _structure_, base interfaces are enforcing _behavior_, every `HandleAsync` method is asynchronous and returns `Result<TData>` structure when _awaited_.
A "subtle" try to move away (developers) from exception-driven control flows when applying CQS principle via similar set of generic interfaces.

Nothing prevents developers to keep using such flow. However, due to implicit conversion it's way more convenient to just embrace the new struct and easily move away from the usually bad practice.

Any "exceptional" state, instead, can be represented with simple inheritable `Error` class.

To represent absence of data, use `Unit` lightweight struct.

## Bus

To get rid of possible annoyances when working with many different generic request handler interfaces use `Bus` default implementation. 
A simple `IBus` dependency easily replaces all kinds of different `IRequestHandler<TRequest, TResponse>` objects which you'd need to use or inject.

It doesn't capture instances and is preferrably used with dependency injection container.

Examples:
```
var createUser = new CreateUserRequest { UserName = "foo", Email = "bar@baz"};

//plain
var handler = new CreateUserRequestHandler(/*_userStore*/);
Result<User> result = await handler.HandleAsync(createUser);

//dependency injection
var handler = _container.GetInstance<IRequestHandler<CreateUserRequest, User>>();
Result<User> result = await handler.HandleAsync(createUser);

//dependency injection with IBus
var bus = _container.GetInstance<IBus>();
Result<User> result = await bus.SendAsync(createUser);

```

## Composition with SimpleInjector

SimpleInjector DI container is used to manage instances, their lifetime and to add cross-cutting concerns as decorators:

```
async Task Main(string[] args)
{
    // setup
    Assembly[] assemblies = // assemblies to scan for request handlers

    var container = new Container();
    container.Register(typeof(IRequestHandler<,>), assemblies);
    container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));

    var bus = container.GetInstance<IBus>();

    // usage
    var createUser = new CreateUserRequest { UserName = "foo", Email = "bar@baz"};
    Result<User> createUserResult = await bus.SendAsync(createUser);

    var updateUser = new UpdateUserRequest { UserName = "foo2", Email = "bar@baz"};
    Result<User> updateUserResult = await bus.SendAsync(updateUser);

    var deleteUser = new DeleteUserRequest { Email = "bar@baz"};
    Result<Unit> deleteUserResult = await bus.SendAsync(deleteUser);
    
}
```

Once result is returned, it can be easily transformed in something else:

```
public class TestController : ApiController
{
    private readonly IBus _bus;    

    public TestController(IBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken ct)
    {
        Result<User> createUserResult = await _bus.Send(request, ct);
        
        return createUserResult.Match(
            user =>  new OkObjectResult(user), 
            error => new BadRequestObjectResult(error)
        );
    }
}

```



