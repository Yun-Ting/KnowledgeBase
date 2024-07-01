﻿// ---------------------------------------------------------------------------------------
// <copyright file="MetaRpProxyProviderClient.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------
namespace Microsoft.AzureStackHCI.ServiceCommon.Services;

using Microsoft.AzureStackHCI.Common.Extensions;
using Microsoft.AzureStackHCI.Common.Models;
using Microsoft.AzureStackHCI.Common.Util;
using Microsoft.AzureStackHCI.ServiceCommon.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Microsoft.AzureStackHCI.Common.Models.LogMessageTemplateConstants;
using Cluster = Common.Models.Cluster;

/// <summary>
/// Http-Client for accessing MetaRP proxy ARM api
/// </summary>
public interface IMetaRpProxyProviderClient
{
    /// <summary>
    /// The interface to trigger the admin delete call of Rpaas From metaRp Proxy
    /// </summary>
    Task TriggerRPaasAdminDeleteApi(RpaasAdminDeleteResource rpaasAdminDeleteResource);

    /// <summary>
    /// Get operation for Cluster resources from meta RP Proxy.
    /// </summary>
    Task<(List<Cluster> clusters, string nextLink)> GetClusterResources(string region, string skipToken);

    /// <summary>
    /// Get operation for update summaries for a cluster resource from meta RP Proxy
    /// </summary>
    Task<UpdateSummaries> GetUpdateSummary(string tenantId, string resourceId, string apiVersion);

    /// <summary>
    /// Get operation for update runs list for a cluster resource from meta RP Proxy
    /// </summary>
    Task<List<UpdateRuns>> GetUpdateRuns(string tenantId, string resourceId, string apiVersion);

    /// <summary>
    /// Get operation for updates list for a cluster resource from meta RP Proxy
    /// </summary>
    Task<List<Update>> GetUpdates(string tenantId, string resourceId, string apiVersion);
}

public class MetaRpProxyProviderClient : IMetaRpProxyProviderClient
{
    private readonly ILogger<MetaRpProxyProviderClient> logger;
    private readonly HttpClient httpClient;
    private readonly IHKMetaRpProxyAuthProvider hkMetaRpProxyAuthProvider;

    private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public MetaRpProxyProviderClient(ILogger<MetaRpProxyProviderClient> logger, HttpClient httpClient, IHKMetaRpProxyAuthProvider authProvider)
    {
        logger.GuardNull();
        httpClient.GuardNull();
        authProvider.GuardNull();
        this.logger = logger;
        this.httpClient = httpClient;
        this.hkMetaRpProxyAuthProvider = authProvider;
    }

    public async Task TriggerRPaasAdminDeleteApi(RpaasAdminDeleteResource rpaasAdminDeleteResource)
    {
        rpaasAdminDeleteResource.GuardNull();
        string endpoint = "rpaas/deleteResources";
        logger.LogInformation($"Triggering Rpaas Admin Delete call for endpoint {endpoint} and resource {rpaasAdminDeleteResource}");
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        await CommonUtils.AuthenticateRequestFromHk(hkMetaRpProxyAuthProvider, request, null);
        logger.LogInformation($"Successfully authenticated the request for Admin Apis");

        string serializedProperties = JsonSerializer.Serialize(rpaasAdminDeleteResource, serializerOptions);
        logger.LogInformation($"Rpaas Admin Delete resource obtained after serializing {serializedProperties}");

        request.Content = new StringContent(serializedProperties);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage result = null;
        try
        {
            result = await httpClient.SendAsync(request);
        }
        catch (HttpRequestException e)
        {
            string message = $"Failed to trigger admin delete api for resource {rpaasAdminDeleteResource}";
            logger.LogError(e, message);
            throw new ResponseException(
                statusCode: e.StatusCode.GetDefaultStatusCode(),
                errorCode: ErrorCode.RpaasAdminApiDeleteFailed,
                target: CommonConstants.MetaRpProxyRpaaSAdminDeleteAPITarget,
                message: message,
                details: CommonUtils.CreateErrorDetails(e.Message, e.StackTrace, e.InnerException),
                innerException: e
            );

        }

        if (result.IsSuccessStatusCode)
        {
            logger.LogInformation("received success response for : {requestURI}", request.RequestUri);
            return;
        }
        else
        {
            string response = await result.Content.ReadAsStringAsync();
            logger.LogError($"POST Rpaas Admin Delete API on {endpoint} failed. Response: {result.StatusCode} {response}");

            ErrorResponse errorResponse = CommonUtils.TryReadErrorResponse(response, serializerOptions);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ResponseException(
                    statusCode: HttpStatusCode.NotFound,
                    errorCode: ErrorCode.RpaasAdminDeleteApiNotFound,
                    target: CommonConstants.MetaRpProxyRpaaSAdminDeleteAPITarget,
                    message: $"Failed to trigger delete call using admin api, Resources = {rpaasAdminDeleteResource}, endpoint = {endpoint}"
                );
            }
            else
            {
                throw new ResponseException(
                    statusCode: result.StatusCode,
                    errorCode: ErrorCode.RpaasAdminApiDeleteFailed,
                    target: CommonConstants.MetaRpProxyRpaaSAdminDeleteAPITarget,
                    message: $"Failed to trigger delete call using admin api, Resources = {rpaasAdminDeleteResource}, endpoint = {endpoint}",
                    details: errorResponse != null ? new List<ErrorDetail> { errorResponse.Error } : null
                );
            }
        }
    }

    public async Task<(List<Cluster> clusters, string nextLink)> GetClusterResources(string region, string skipToken = null)
    {
        string endpoint = skipToken != null
            ? $"rpaas/gethciresourcesbyfilter?filter=Location eq '{region}'&skipToken={skipToken}"
            : $"rpaas/gethciresourcesbyfilter?filter=Location eq '{region}'";

        logger.LogInformation($"Trying to fetch [{EmptyPlaceholder}] resources from meta RP Proxy [{EmptyPlaceholder}]", typeof(Cluster), endpoint);

        string errorMessage = $"Could not retrieve {typeof(Cluster)} resources from meta RP Proxy.";

        try
        {
            ResourceCollection<Cluster> clustersPage = await GetCollectionPage<Cluster>(endpoint);

            if (clustersPage == null)
            {
                logger.LogInformation($"No cluster resource found on endpoint [{LogMessageTemplateConstants.Endpoint}] in meta RP", endpoint);
                return (null, null);
            }

            List<Cluster> clusters = clustersPage.Value.ToList();
            string nextLink = clustersPage.NextLink;

            logger.LogInformation("Number of clusters returned from metarp proxy in this page [{}] ", clusters.Count);

            return (clusters, nextLink);
        }
        catch (HttpRequestException e)
        {
            throw new ResponseException(
                statusCode: e.StatusCode.GetDefaultStatusCode(),
                errorCode: ErrorCode.RpaasAdminGetClustersApiFailed,
                target: endpoint,
                message: errorMessage,
                details: CommonUtils.CreateErrorDetails(e.Message, e.StackTrace, e.InnerException),
                innerException: e
            );
        }
        catch (ResponseException e)
        {
            throw new ResponseException(
                statusCode: e.StatusCode,
                errorCode: ErrorCode.RpaasAdminGetClustersApiFailed,
                target: endpoint,
                message: errorMessage,
                details: e.Details
            );
        }
    }

    public async Task<UpdateSummaries> GetUpdateSummary(string tenantId, string resourceId, string apiVersion)
    {
        string endpoint = $"rpaas/getupdatesummary?resourceId={resourceId}&api-version={apiVersion}";
        logger.LogInformation($"Trying to fetch [{EmptyPlaceholder}] resources from meta RP Proxy [{EmptyPlaceholder}]", typeof(UpdateSummaries), endpoint);

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("x-ms-cluster-client-tenant-id", tenantId);

        try
        {
            await CommonUtils.AuthenticateRequestFromHk(hkMetaRpProxyAuthProvider, request, null);
            using HttpResponseMessage result = await httpClient.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                UpdateSummaries updateSummaries = await result.Content.ReadAsJsonAsync<UpdateSummaries>(serializerOptions);

                logger.LogInformation($"Successfully fetched cluster updates summary from meta RP Proxy, IsNull: {updateSummaries == null}");

                return updateSummaries;
            }

            // Handle errors
            string response = await result.Content.ReadAsStringAsync();

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation($"No update summary found in meta RP for resource {resourceId}");

                return null;
            }

            throw new ResponseException(
            statusCode: result.StatusCode,
            errorCode: ErrorCode.GetUpdatesResourceFailed,
            message: "Failed to retrieve update summaries",
            details: CreateErrorDetails(result.StatusCode, response)
            );
        }
        catch (HttpRequestException e)
        {
            string message = $"Failed to retrieve update summaries for resource {resourceId}";
            logger.LogError(e, message);
            throw new ResponseException(
                statusCode: e.StatusCode.GetDefaultStatusCode(),
                errorCode: ErrorCode.InternalHttpClientError,
                message: message,
                details: CommonUtils.CreateErrorDetails(e.Message, e.StackTrace, e.InnerException),
                innerException: e
            );
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to retrieve update summaries for resource {resourceId}, exception occurred: {ex}");
            throw;
        }
    }

    public async Task<List<Update>> GetUpdates(string tenantId, string resourceId, string apiVersion)
    {
        string endpoint = $"rpaas/getupdates?resourceId={resourceId}&api-version={apiVersion}";
        logger.LogInformation($"Trying to fetch [{EmptyPlaceholder}] resources from meta RP Proxy [{EmptyPlaceholder}]", typeof(Update), endpoint);

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("x-ms-cluster-client-tenant-id", tenantId);

        try
        {
            await CommonUtils.AuthenticateRequestFromHk(hkMetaRpProxyAuthProvider, request, null);
            using HttpResponseMessage result = await httpClient.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                List<Update> updates = await result.Content.ReadAsJsonAsync<List<Update>>(serializerOptions);

                logger.LogInformation($"Successfully fetched updates list from meta RP Proxy, count: {updates?.Count()}");

                return updates;
            }

            // Handle errors
            string response = await result.Content.ReadAsStringAsync();

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation($"No updates found in meta RP for resource {resourceId}");

                return null;
            }

            throw new ResponseException(
                statusCode: result.StatusCode,
                errorCode: ErrorCode.GetUpdatesResourceFailed,
                message: "Failed to retrieve updates list.",
                details: CreateErrorDetails(result.StatusCode, response)
            );
        }
        catch (HttpRequestException e)
        {
            string message = $"Failed to retrieve updates for resource {resourceId}";
            logger.LogError(e, message);
            throw new ResponseException(
                statusCode: e.StatusCode.GetDefaultStatusCode(),
                errorCode: ErrorCode.InternalHttpClientError,
                message: message,
                details: CommonUtils.CreateErrorDetails(e.Message, e.StackTrace, e.InnerException),
                innerException: e
            );
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to retrieve updates for resource {resourceId}, exception occurred: {ex}");
            throw;
        }
    }

    public async Task<List<UpdateRuns>> GetUpdateRuns(string tenantId, string resourceId, string apiVersion)
    {
        string endpoint = $"rpaas/getupdateruns?resourceId={resourceId}&api-version={apiVersion}";
        logger.LogInformation($"Trying to fetch [{EmptyPlaceholder}] resources from meta RP Proxy [{EmptyPlaceholder}]", typeof(UpdateRuns), endpoint);

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("x-ms-cluster-client-tenant-id", tenantId);

        try
        {
            await CommonUtils.AuthenticateRequestFromHk(hkMetaRpProxyAuthProvider, request, null);
            using HttpResponseMessage result = await httpClient.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                List<UpdateRuns> updateRuns = await result.Content.ReadAsJsonAsync<List<UpdateRuns>>(serializerOptions);

                logger.LogInformation($"Successfully fetched update runs list from meta RP Proxy, count: {updateRuns?.Count()}");

                return updateRuns;
            }

            // Handle errors
            string response = await result.Content.ReadAsStringAsync();

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation($"No update runs found in meta RP for resource {resourceId}");

                return null;
            }

            throw new ResponseException(
                statusCode: result.StatusCode,
                errorCode: ErrorCode.GetUpdatesResourceFailed,
                message: "Failed to retrieve update runs.",
                details: CreateErrorDetails(result.StatusCode, response)
            );
        }
        catch (HttpRequestException e)
        {
            string message = $"Failed to retrieve update runs for resource {resourceId}";
            logger.LogError(e, message);
            throw new ResponseException(
                statusCode: e.StatusCode.GetDefaultStatusCode(),
                errorCode: ErrorCode.InternalHttpClientError,
                message: message,
                details: CommonUtils.CreateErrorDetails(e.Message, e.StackTrace, e.InnerException),
                innerException: e
            );
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to retrieve update runs for resource {resourceId}, exception occurred: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Helper method for iterating over collections. Caller needs to handle HttpRequestException
    /// and ResponseException.
    /// </summary>
    /// <param name="endpoint">URL to get a collection page from (value of NextLink property)</param>
    private async Task<ResourceCollection<Cluster>> GetCollectionPage<Cluster>(string endpoint)
    {
        logger.LogInformation($"Trying to fetch cluster collection on endpoint [{LogMessageTemplateConstants.Endpoint}] from meta RP", endpoint);

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        await CommonUtils.AuthenticateRequestFromHk(hkMetaRpProxyAuthProvider, request, null);
        HttpResponseMessage result = await httpClient.SendAsync(request);

        JsonSerializerOptions clusterSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        clusterSerializerOptions.Converters.Add(new IgnoreParsingErrorConverter<Cluster>(logger));
        if (result.IsSuccessStatusCode)
        {
            string responseBody = await result.Content.ReadAsStringAsync();
            ResourceCollection<Cluster> resourceCollection = JsonSerializer.Deserialize<ResourceCollection<Cluster>>(responseBody, clusterSerializerOptions);

            logger.LogInformation($"Successfully fetched cluster collection from meta RP");

            return resourceCollection;
        }

        // Handle errors
        string response = await result.Content.ReadAsStringAsync();

        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("No cluster collection found in meta RP");

            return null;
        }

        throw new ResponseException(
            statusCode: result.StatusCode,
            errorCode: ErrorCode.InternalHttpClientError,
            message: "Failed to retrieve a collection page.",
            details: CreateErrorDetails(result.StatusCode, response)
        );
    }

    private static List<ErrorDetail> CreateErrorDetails(HttpStatusCode statusCode, string message)
    {
        return new List<ErrorDetail>
            {
                new ErrorDetail
                {
                    Code = ErrorCode.InternalHttpClientError,
                    Message = $"({(int)statusCode} {statusCode}) {message}"
                }
            };
    }

}
