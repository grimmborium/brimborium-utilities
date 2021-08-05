using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Brimborium.RequestHandler {
    public class HandlerResponse {
        public HandlerResponse() : this(200, null) {
        }
        public HandlerResponse([Microsoft.AspNetCore.Mvc.Infrastructure.ActionResultStatusCode] int statusCode, string? statusMessage = null) {
            this.StatusCode = statusCode;
            this.StatusMessage = statusMessage;
        }

        public int StatusCode { get; set; }
        public string? StatusMessage { get; set; }

        public static HandlerResponse Forbidden(string? statusMessage=default) => new ForbiddenResponseResult(statusMessage);

        public static HandlerResponse BadRequest(string? statusMessage = null) => new BadRequestResponseResult(statusMessage);

        public static HandlerResponse Error(System.Exception exception, string? statusMessage = null) => new ErrorResponseResult(exception, statusMessage);

        public static HandlerResponse ErrorPassthrough(System.Exception exception) => new ErrorPassthroughResponseResult(exception);

        public static HandlerResponse<TValue> Ok<TValue>(TValue value)
            => new HandlerResponse<TValue>(value);
    }

    public class ForbiddenResponseResult : HandlerResponse {
        public ForbiddenResponseResult() : base((int)HttpStatusCode.Forbidden) {
        }
        public ForbiddenResponseResult(string? statusMessage) : base((int)HttpStatusCode.Forbidden, statusMessage) {
        }
    }

    public class BadRequestResponseResult : HandlerResponse {
        public BadRequestResponseResult(string? statusMessage = null) : base((int)HttpStatusCode.BadRequest, statusMessage) {
        }
    }

    public class ErrorResponseResult : BadRequestResponseResult {
        public ErrorResponseResult(System.Exception exception, string? statusMessage = null) : base(statusMessage) {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }

    public class ErrorPassthroughResponseResult : HandlerResponse {
        public ErrorPassthroughResponseResult(System.Exception exception, string? statusMessage = null) : base(500, statusMessage) {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
    /*
    public class XResponseResult : HandlerResponse {
        public XResponseResult() : base((int)HttpStatusCode.Forbidden) {
        }
    }
    */

    [Serializable]
    public sealed class HandlerResponse<TValue> {
        private int _ValueOrResult;
        private TValue? _Value;
        private HandlerResponse? _Result;

        public bool HasValue => this._ValueOrResult == 1 && this._Value is object;

        public bool HasResult => this._ValueOrResult == 2 && this._Result is object;

        public HandlerResponse? Result {
            get {
                if (this._ValueOrResult == 2) {
                    return this._Result;
                }
                return default;
            }
            set {
                if (value is null && this._Result is null) {
                    // ignore
                } else if (this._Result is object) {
                    throw new InvalidOperationException("Result is already set");
                } else if (this._Value is object) {
                    throw new InvalidOperationException("Value is already set");
                } else {
                    this._ValueOrResult = 2;
                    this._Result = value;
                }
            }
        }

        public TValue? Value {
            get {
                if (this._ValueOrResult == 1) {
                    return this._Value;
                }
                return default;
            }
            set {
                if (value is null && this._Value is null) {
                    // ignore
                } else if (this._Result is object) {
                    throw new InvalidOperationException("Result is already set");
                } else if (this._Value is object) {
                    throw new InvalidOperationException("Value is already set");
                } else {
                    this._ValueOrResult = 1;
                    this._Value = value;
                }
            }
        }

        public HandlerResponse() {
        }

        public HandlerResponse(TValue value) {
            this._ValueOrResult = 1;
            this._Value = value;
        }

        public HandlerResponse(HandlerResponse result) {
            this._ValueOrResult = 2;
            this._Result = result;
        }

        public TValue GetValueOrFail() {
            if (this._ValueOrResult == 1) {
                if (this._Value is null) {
                    throw new InvalidCastException($"{typeof(TValue).FullName} is null.");
                } else {
                    return this._Value;
                }
            }
            if (this._ValueOrResult == 2) {
                throw new InvalidCastException($"No {typeof(TValue).FullName} -Value, but a Result.");
            } else {
                throw new InvalidCastException($"No {typeof(TValue).FullName}-Value - nor a Result.");

            }
        }

        public bool TryGetValue([MaybeNullWhen(false)] out TValue value) {
            value = this._Value;
            return this._ValueOrResult == 1;
        }

        public bool TryGetResult([MaybeNullWhen(false)] out HandlerResponse? result) {
            result = this._Result;
            return this._ValueOrResult == 2;
        }

        public static implicit operator HandlerResponse<TValue>(TValue value)
            => new HandlerResponse<TValue>(value);

        public static implicit operator HandlerResponse<TValue>(HandlerResponse result)
            => new HandlerResponse<TValue>(result);
    }

    public static class HandlerResponseExtension {
        public static HandlerResponse<TValue> AsHandlerResponseResult<TValue>(this HandlerResponse that) {
            return new HandlerResponse<TValue>(that);
        }

        public static HandlerResponse<TValue> AsHandlerResponseValue<TValue>(this TValue that) {
            return new HandlerResponse<TValue>(that);
        }

        public static ActionResult<TValue> Returns<TValue>(this HandlerResponse<TValue> that) {
            if (that.HasValue) {
                return new ActionResult<TValue>(that.Value!);
            } else {
                return new ActionResult<TValue>(that.Result.Convert());
            }
        }
        public static ActionResult<TResult> Returns<TValue, TResult>(this HandlerResponse<TValue> that, Func<TValue, TResult> getResult) {
            if (that.HasValue) {
                return new ActionResult<TResult>(getResult(that.Value!));
            } else {
                return new ActionResult<TResult>(that.Result.Convert());
            }
        }
        public static ActionResult Convert(this HandlerResponse? that) {
            if (that is object) {
                // return new Microsoft.AspNetCore.Mvc.ObjectResult(that.StatusCode);
                return new Microsoft.AspNetCore.Mvc.StatusCodeResult(that.StatusCode);
            } else {
                return new Microsoft.AspNetCore.Mvc.StatusCodeResult(500);
            }
        }

#warning thinkof: remove
        public static void HandleResponse<TValue, TArgs>(
            this HandlerResponse<TValue> that,
            TArgs args,
            Action<TArgs, TValue>? onSuccess = default,
            Action<TArgs, HandlerResponse>? onFailure = default,
            Action<TArgs>? onNone = default) {
            if (that.HasValue) {
                if (onSuccess is object) {
                    onSuccess(args, that.Value!);
                }
            } else if (that.HasResult) {
                if (onFailure is object) {
                    onFailure(args, that.Result!);
                }
            } else {
                if (onNone is object) {
                    onNone(args);
                }
            }
        }

        public static TResult Map<TValue, TResult>(
            this HandlerResponse<TValue> that,
            Func<TValue, TResult> onSuccess,
            Func<HandlerResponse, TResult>? onFailure = default,
            Func<TResult>? onNone = default) {
            if (that.HasValue) {
                return onSuccess(that.Value!);
            } else if (that.HasResult) {
                if (onFailure is object) {
                    return onFailure(that.Result!);
                }
            } else {
                if (onNone is object) {
                    return onNone();
                }
            }
            throw new InvalidOperationException("case not defined.");
        }

        public static TResult Map<TValue, TArgs, TResult>(
            this HandlerResponse<TValue> that,
            TArgs args,
            Func<TArgs, TValue, TResult> onSuccess,
            Func<TArgs, HandlerResponse, TResult>? onFailure = default,
            Func<TArgs, TResult>? onNone = default) {
            if (that.HasValue) {
                return onSuccess(args, that.Value!);
            } else if (that.HasResult) {
                if (onFailure is object) {
                    return onFailure(args, that.Result!);
                }
            } else {
                if (onNone is object) {
                    return onNone(args);
                }
            }
            throw new InvalidOperationException("case not defined.");
        }
    }
}
