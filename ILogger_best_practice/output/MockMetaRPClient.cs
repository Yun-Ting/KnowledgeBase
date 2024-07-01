// ---------------------------------------------------------------------------------------
// <copyright file="MockMetaRPClient.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

namespace Microsoft.AzureStackHCI.ServiceCommon.Services;

using Microsoft.AzureStackHCI.Common.Models;
using Microsoft.AzureStackHCI.Common.Util;
using Microsoft.AzureStackHCI.ServiceCommon.Converters;
using Microsoft.AzureStackHCI.ServiceCommon.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;

[ExcludeFromCodeCoverage]
public class MockMetaRPClient : MetaRPClient
{
    private const string EdgeDevices = "EdgeDevices";

    private readonly IDistributedCache distributedCache;

    private readonly ILogger<MockMetaRPClient> mockMetaRpClientLogger;

    public MockMetaRPClient(ILogger<MetaRPClient> logger,
                            HttpClient httpClient,
                            IAuthenticationProvider authProvider,
                            IOptionsMonitor<RPaaSOptions> rpaasOptionsAccessor,
                            IHostEnvironment env,
                            IDistributedCache distributedCache,
                            ILogger<MockMetaRPClient> mockMetaRpClientLogger) : base(logger, httpClient, authProvider, rpaasOptionsAccessor, env)
    {
        this.distributedCache = distributedCache;
        this.mockMetaRpClientLogger = mockMetaRpClientLogger;

        this.mockMetaRpClientLogger.LogInformation("Initialized [{}]", nameof(MockMetaRPClient));
    }

    public override async Task<EdgeDevice> GetEdgeDevice(string tenantId, string resourceId, string apiVersion = null)
    {
        mockMetaRpClientLogger.LogInformation("Getting Edge devices [{}]", resourceId);

        IList<EdgeDevice> edgeDevices = await GetEdgeDevicesFromCache();

        return edgeDevices?.First(d => resourceId.Equals(d.Id, StringComparison.OrdinalIgnoreCase));
    }

    public override async Task PutEdgeDevice(string tenantId, EdgeDevice edgeDevice, string apiVersion = null)
    {
        mockMetaRpClientLogger.LogInformation("Putting edge device [{}]", edgeDevice);

        List<EdgeDevice> currentEdgeDevices = (await GetEdgeDevicesFromCache()).ToList();
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EdgeDeviceJsonConverter());
        bool flag = false;
        foreach (EdgeDevice device in currentEdgeDevices)
        {
            string currentSerializedDevice = JsonSerializer.Serialize(device, options);
            mockMetaRpClientLogger.LogInformation("Current Edge Device [{}]", currentSerializedDevice);
            string newSerializedDevice = JsonSerializer.Serialize(edgeDevice, options);
            mockMetaRpClientLogger.LogInformation("Getting Edge devices [{}]", newSerializedDevice);
            if (currentSerializedDevice.Equals(newSerializedDevice))
            {
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            throw new ResponseException(
                    statusCode: HttpStatusCode.BadRequest,
                    errorCode: ErrorCode.ValidationFailed,
                    message: "The current edge device is not present in cache so invalid payload"
                    );
        }
    }

    public override async Task<List<EdgeDevice>> GetEdgeDevicesBatchRequest(string tenantId, HashSet<string> edgeDeviceResourceIds, string apiVersion = null)
    {
        mockMetaRpClientLogger.LogInformation("Getting Edge devices [{}]", CommonUtils.CollectionToString(edgeDeviceResourceIds));

        var edgeDevices = await GetEdgeDevicesFromCache();

        return edgeDevices == null
               ? null
               : (from device in edgeDevices
                  where edgeDeviceResourceIds.Contains(device.Id)
                  select device).ToList();
    }

    public async Task SeedCacheData(IList<DefaultEdgeDevice> edgeDevices)
    {
        if (edgeDevices == null)
        {
            mockMetaRpClientLogger.LogInformation("Edge devices should not be null, settting to empty list");

            edgeDevices = new List<DefaultEdgeDevice>();
        }

        mockMetaRpClientLogger.LogInformation("Seeding default edge devices data in cache");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EdgeDeviceJsonConverter());
        string data = JsonSerializer.Serialize(edgeDevices, options);
        mockMetaRpClientLogger.LogInformation($"Seeding default edge device data [{data}]");
        await distributedCache.SetStringAsync(EdgeDevices, data);
    }

    public async Task SeedHciEdgeDevicesCacheData(IList<HciEdgeDevice> edgeDevices)
    {
        if (edgeDevices == null)
        {
            mockMetaRpClientLogger.LogInformation("Edge devices should not be null, settting to empty list");

            edgeDevices = new List<HciEdgeDevice>();
        }

        mockMetaRpClientLogger.LogInformation("Seeding HCI edge devices data in cache");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EdgeDeviceJsonConverter());
        string data = JsonSerializer.Serialize(edgeDevices, options);
        mockMetaRpClientLogger.LogInformation($"Seeding HCI edge device data [{data}]");
        await distributedCache.SetStringAsync(EdgeDevices, data);
    }

    public async Task ClearCacheData()
    {
        await distributedCache.RemoveAsync(EdgeDevices);
    }

    private async Task<IList<EdgeDevice>> GetEdgeDevicesFromCache()
    {
        mockMetaRpClientLogger.LogInformation("Trying to fetch edge devices from cache");
        string serializedData = await distributedCache.GetStringAsync(EdgeDevices);
        mockMetaRpClientLogger.LogInformation("Fetched edge devices from cache: [{}]", serializedData);
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EdgeDeviceJsonConverter());
        return !string.IsNullOrEmpty(serializedData) && serializedData != "[]" ? JsonSerializer.Deserialize<List<EdgeDevice>>(serializedData, options) : null;
    }
}
