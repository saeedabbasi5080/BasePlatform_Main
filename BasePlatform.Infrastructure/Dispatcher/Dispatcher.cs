using BasePlatform.Application.Common.Abstractions;
using BasePlatform.Shared;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BasePlatform.Infrastructure.Dispatcher;

public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command);
        if (validationError is not null)
            return CreateFailure<TResult>(validationError);

        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("HandleAsync")!;
        var task = (Task<TResult>)method.Invoke(handler, [command, cancellationToken])!;

        return await task;
    }

    public async Task SendAsync(
        ICommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(command);
        if (validationError is not null)
            return;

        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(Result));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("HandleAsync")!;
        var task = (Task<Result>)method.Invoke(handler, [command, cancellationToken])!;

        await task;
    }

    public async Task<TResult> QueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("HandleAsync")!;
        var task = (Task<TResult>)method.Invoke(handler, [query, cancellationToken])!;

        return await task;
    }

    /// <summary>
    /// Runs any registered FluentValidation validators for the command type.
    /// Returns a combined error message when validation fails, otherwise null.
    /// </summary>
    private Error? Validate(object command)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var validators = _serviceProvider.GetServices(validatorType)
            .Cast<IValidator>()
            .ToList();

        if (validators.Count == 0)
            return null;

        var context = new ValidationContext<object>(command);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return null;

        var message = string.Join("; ", failures.Select(f => f.ErrorMessage));

        var fieldErrors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => ToCamelCase(g.Key),
                g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        return Error.Validation("Validation.Failed", message, fieldErrors);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            return value;

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static TResult CreateFailure<TResult>(Error error)
    {
        var resultType = typeof(TResult);

        if (resultType == typeof(Result))
            return (TResult)(object)Result.Failure(error);

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = resultType.GetMethod("Failure", [typeof(Error)])!;
            return (TResult)failureMethod.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"Validation failed but result type '{resultType}' is not a Result type.");
    }
}
