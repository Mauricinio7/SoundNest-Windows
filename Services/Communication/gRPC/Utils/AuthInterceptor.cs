using Grpc.Core.Interceptors;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Grpc.Core.Interceptors.Interceptor;

namespace Services.Communication.gRPC.Utils
{
    public class AuthInterceptor : Interceptor
    {
        private readonly string _token;

        public AuthInterceptor(string token)
        {
            _token = token;
        }

        private CallOptions AddAuthorizationHeader<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
        {
            var headers = context.Options.Headers ?? new Metadata();
            headers.Add("authorization", $"Bearer {_token}");

            return context.Options.WithHeaders(headers);
        }

        // Unary
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host, AddAuthorizationHeader(context));

            return base.AsyncUnaryCall(request, newContext, continuation);
        }

        // Server streaming
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host, AddAuthorizationHeader(context));

            return base.AsyncServerStreamingCall(request, newContext, continuation);
        }

        // Client streaming
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host, AddAuthorizationHeader(context));

            return base.AsyncClientStreamingCall(newContext, continuation);
        }

        // Bidirectional streaming
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host, AddAuthorizationHeader(context));

            return base.AsyncDuplexStreamingCall(newContext, continuation);
        }
    }
}
