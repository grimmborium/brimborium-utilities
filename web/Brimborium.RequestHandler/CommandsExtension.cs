using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

namespace Brimborium.RequestHandler {
    public static class CommandsExtension {
        public static TRequestHandler CreateRequestHandler<TRequestHandler>(
                this Microsoft.AspNetCore.Mvc.RazorPages.PageModel pageModel
            )
            where TRequestHandler : notnull {
            var requestServices = pageModel.HttpContext.RequestServices;
            var requestHandlerFactory = requestServices.GetRequiredService<IRequestHandlerFactory>();
            var requestHandler = requestHandlerFactory.CreateRequestHandler<TRequestHandler>(requestServices);
            return requestHandler;
        }

        public static TRequestHandler CreateRequestHandler<TRequestHandler>(
                this Microsoft.AspNetCore.Mvc.ControllerBase controllerBase
            )
            where TRequestHandler : notnull {
            var requestServices = controllerBase.HttpContext.RequestServices;
            var requestHandlerFactory = requestServices.GetRequiredService<IRequestHandlerFactory>();
            var requestHandler = requestHandlerFactory.CreateRequestHandler<TRequestHandler>(requestServices);
            return requestHandler;
        }

        public static async Task<TResponse> ExecuteAsync<TRequestHandler, TRequest, TResponse>(
            this Microsoft.AspNetCore.Mvc.ControllerBase controllerBase,
            TRequest request
            )
            where TRequestHandler : notnull, IRequestHandler<TRequest, TResponse> {
            var requestServices = controllerBase.HttpContext.RequestServices;
            var requestHandlerFactory = requestServices.GetRequiredService<IRequestHandlerFactory>();
            var requestHandler = requestHandlerFactory.CreateRequestHandler<TRequestHandler>(requestServices);
            return await requestHandler.ExecuteAsync(request);
        }

        public static TRequestHandler CreateRequestHandler<TRequestHandler>(
                this IServiceProvider scopedServiceProvider
            )
            where TRequestHandler : notnull {
            var requestHandlerFactory = scopedServiceProvider.GetRequiredService<IRequestHandlerFactory>();
            var requestHandler = requestHandlerFactory.CreateRequestHandler<TRequestHandler>(scopedServiceProvider);
            return requestHandler;
        }
    }
}