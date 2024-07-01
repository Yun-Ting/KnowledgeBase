// ---------------------------------------------------------------------------------------
// <copyright file="CorrelationMessageHandler.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

namespace Microsoft.AzureStackHCI.ServiceCommon.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AzureStackHCI.Common.Models;

/// <summary>
/// Adds correlation ids: x-ms-request-id and x-ms-correlation-request-id to request headers.
/// These ids need to be present in http context:
/// request id is read from context.TraceIdentifier
/// correlation id is read from context.Items["x-ms-correlation-request-id"]
/// </summary>
public class CorrelationMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;

    private readonly ICorrelationContext correlationContext;
    // If mocking for testing is cumbersome, create ICorrelationAccessor.
    public CorrelationMessageHandler(IHttpContextAccessor httpContextAccessor, ICorrelationContext correlationContext)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.correlationContext = correlationContext;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (httpContextAccessor?.HttpContext != null)
        {
            request.Headers.Add(CommonConstants.RequestIdHeader, httpContextAccessor.HttpContext.TraceIdentifier);
            request.Headers.Add(CommonConstants.CorrelationIdHeader, httpContextAccessor.HttpContext.Items[CommonConstants.CorrelationIdHeader] as string);
        }
        else
        {
            // TODO: Investigate if we need requestId header
            request.Headers.Add(CommonConstants.CorrelationIdHeader, correlationContext.CorrelationId);
        }


        return base.SendAsync(request, cancellationToken);
    }
}
