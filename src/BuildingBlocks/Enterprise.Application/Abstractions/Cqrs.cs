using MediatR;

namespace Enterprise.Application.Abstractions;

public interface ICommandBase;

public interface ICommand : IRequest, ICommandBase;

public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase;

public interface IQuery<out TResponse> : IRequest<TResponse>;
