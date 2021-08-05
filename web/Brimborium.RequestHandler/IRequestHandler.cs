using System.Threading;
using System.Threading.Tasks;

namespace Brimborium.RequestHandler {
    public interface IRequestHandler<TRequest, TResponse> {
        Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
    }
    public interface IMvcActionRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, Microsoft.AspNetCore.Mvc.ActionResult<TResponse>> { }
    public interface IActionRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, HandlerResponse<TResponse>> { }
}