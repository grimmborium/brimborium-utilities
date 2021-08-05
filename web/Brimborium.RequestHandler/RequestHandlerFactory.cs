using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;

namespace Brimborium.RequestHandler {

    public class RequestHandlerFactory : IRequestHandlerFactory {
        private static Dictionary<Type, bool> _TypedRequestHandler = new Dictionary<Type, bool>();
        private readonly IServiceProvider _ServiceProvider;

        public RequestHandlerFactory(
            IServiceProvider serviceProvider
            ) {
            this._ServiceProvider = serviceProvider;
        }

        public TRequestHandler CreateRequestHandler<TRequestHandler>(
                IServiceProvider scopedServiceProvider
            )
            where TRequestHandler : notnull {
            var found = _TypedRequestHandler.TryGetValue(typeof(TRequestHandler), out var isTypedRequestHandler);
            if (!found || (found && isTypedRequestHandler)) {
                var typedRequestHandlerFactory = this._ServiceProvider.GetService<ITypedRequestHandlerFactory<TRequestHandler>>();
                if (typedRequestHandlerFactory is object) {
                    if (!found) {
                        setTypedRequestHandler(true);
                    }
                    var requestHandler = typedRequestHandlerFactory.CreateTypedRequestHandler(scopedServiceProvider);
                    return requestHandler;
                } else {
                    if (!found) {
                        setTypedRequestHandler(true);
                    }
                }
            }

            {
                var requestHandler = scopedServiceProvider.GetRequiredService<TRequestHandler>();
                return requestHandler;
            }

            void setTypedRequestHandler(bool value) {
                var n = new Dictionary<Type, bool>(_TypedRequestHandler);
                n[typeof(TRequestHandler)] = value;
                _TypedRequestHandler = n;
            }
        }
    }
}