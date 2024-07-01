// ---------------------------------------------------------------------------------------
// <copyright file="CorrelationContext.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

namespace Microsoft.AzureStackHCI.ServiceCommon.Services;

/// <summary>
/// Modifies http request to include properties required for auth. Eg. implementation may add
/// Authorization header.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Add authentication data for default/management tenant, eg. for access to non-user specific
    /// Meta RP endpoints.
    /// </summary>
    string CorrelationId { get; set; }
}

[ExcludeFromCodeCoverage]
public class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; set; }
}
