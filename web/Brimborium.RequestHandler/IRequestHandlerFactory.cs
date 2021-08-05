using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;

namespace Brimborium.RequestHandler {
    public interface IRequestHandlerFactory {
        TRequestHandler CreateRequestHandler<TRequestHandler>(
                IServiceProvider scopedServiceProvider
            )
            where TRequestHandler : notnull;
    }

    public interface ITypedRequestHandlerFactory<TRequestHandler>
        where TRequestHandler : notnull {
        TRequestHandler CreateTypedRequestHandler(
            IServiceProvider scopedServiceProvider
            );
    }
}