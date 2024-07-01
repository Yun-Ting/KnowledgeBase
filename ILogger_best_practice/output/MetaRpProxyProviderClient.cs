// ---------------------------------------------------------------------------------------
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
 using System;

 internal static partial class LoggerExtensions
 {
     [LoggerMessage(Level = LogLevel.Information, Message = "Trying to fetch [{resourceType}] resources from meta RP Proxy [{endpoint}]")]
     public static partial void LogFetchingResources(ILogger logger, Type resourceType, string endpoint);

     [LoggerMessage(Level = LogLevel.Information, Message = "No cluster resource found on endpoint [{endpoint}] in meta RP")]
     public static partial void LogNoClusterResourceFound(ILogger logger, string endpoint);

      [LoggerMessage(Level = LogLevel.Information, Message = "Number of clusters returned from metarp proxy in this page [{count}]")]
      public static partial void LogClusterCountFromMetaRp(ILogger logger, int count);
  }

  // Rest of the code remains unchanged...
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
     internal static partial class Log
     {
         [LoggerMessage(Level = LogLevel.Information, Message = "Trying to fetch {type} resources from meta RP Proxy {endpoint}")]
         public static partial void AttemptingToFetchResources(ILogger logger, Type type, string endpoint);

         [LoggerMessage(Level = LogLevel.Information, Message = "Successfully fetched cluster updates summary from meta RP Proxy, IsNull: {isNull}")]
          public static partial void FetchedClusterUpdatesSummary(ILogger logger, bool isNull);

          [LoggerMessage(Level = LogLevel.Information, Message = "No update summary found in meta RP for resource {resourceId}")]
          public static partial void NoUpdateSummaryFound(ILogger logger, string resourceId);

          [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve update summaries for resource {resourceId}")]
          public static partial void FailedToRetrieveUpdateSummaries(ILogger logger, Exception ex, string resourceId);

          [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve update summaries for resource {resourceId}, exception occurred: {ex}")]
          public static partial void FailedToRetrieveUpdateSummariesWithException(ILogger logger, string resourceId, Exception ex);
      }
  }
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Trying to fetch cluster collection on endpoint [{endpoint}] from meta RP")]
        public static partial void LogTryingToFetchClusterCollection(ILogger logger, string endpoint);

        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully fetched cluster collection from meta RP")]
        public static partial void LogSuccessfullyFetchedClusterCollection(ILogger logger);

         [LoggerMessage(Level = LogLevel.Information, Message = "No cluster collection found in meta RP")]
         public static partial void LogNoClusterCollectionFound(ILogger logger);
     }
  // ... (rest of the class code)
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

        LoggerExtensions.LogFetchingResources(logger, typeof(Cluster), endpoint);

        string errorMessage = $"Could not retrieve {typeof(Cluster)} resources from meta RP Proxy.";

        try
        {
            ResourceCollection<Cluster> clustersPage = await GetCollectionPage<Cluster>(endpoint);

            if (clustersPage == null)
            {
                LoggerExtensions.LogNoClusterResourceFound(logger, endpoint);
                return (null, null);
            }

            List<Cluster> clusters = clustersPage.Value.ToList();
            string nextLink = clustersPage.NextLink;

            LoggerExtensions.LogClusterCountFromMetaRp(logger, clusters.Count);

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
        logger.AttemptingToFetchResources(typeof(UpdateSummaries), endpoint);
        // The following partial method will be generated by the source generator.

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("x-ms-cluster-client-tenant-id", tenantId);

        try
        {
            await CommonUtils.AuthenticateRequestFromHk(hkMetaRpProxyAuthProvider, request, null);
            using HttpResponseMessage result = await httpClient.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                UpdateSummaries updateSummaries = await result.Content.ReadAsJsonAsync<UpdateSummaries>(serializerOptions);

                logger.FetchedClusterUpdatesSummary(updateSummaries == null);
                // The following partial method will be generated by the source generator.

                return updateSummaries;
            }

            // Handle errors
            string response = await result.Content.ReadAsStringAsync();

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                logger.NoUpdateSummaryFound(resourceId);
                // The following partial method will be generated by the source generator.

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
            string message = "Failed to retrieve update summaries for resource {resourceId}";
            logger.FailedToRetrieveUpdateSummaries(e, resourceId);
            // The following partial method will be generated by the source generator.
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
            logger.FailedToRetrieveUpdateSummariesWithException(resourceId, ex);
            // The following partial method will be generated by the source generator.
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
        LoggerExtensions.LogTryingToFetchClusterCollection(logger, endpoint);

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

            LoggerExtensions.LogSuccessfullyFetchedClusterCollection(logger);

            return resourceCollection;
        }

        // Handle errors
        string response = await result.Content.ReadAsStringAsync();

        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            LoggerExtensions.LogNoClusterCollectionFound(logger);

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
