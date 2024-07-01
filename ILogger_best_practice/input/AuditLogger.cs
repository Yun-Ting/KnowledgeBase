// ---------------------------------------------------------------------------------------
// <copyright file="AuditLogger.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

namespace Microsoft.AzureStackHCI.Common.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AzureStackHCI.Common.Models;
using Microsoft.AzureStackHCI.ServiceCommon.Models;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Audit.Geneva;

/// <summary>
/// Interface which exposes methods for both logging success and failure of an operation
/// </summary>
public interface IAuditLogger
{
    public void LogSuccess(string resource, string operationName, OperationType operationType, AuditLoggerType auditLoggerType);
    public void LogFailure(string resource, string operationName, OperationType operationType, AuditLoggerType auditLoggerType);
}

/// <summary>
/// class which creates the audit record for both success and failure case and generates opentelemetry events which are captured by the OneDSProvider
/// </summary>
[ExcludeFromCodeCoverage]
public class AuditLogger : IAuditLogger
{
    /// <summary>
    /// HttpContextAccessor used to access httpcontext
    /// </summary>
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Extensions Logger to log data
    /// </summary>
    private readonly ILogger<AuditLogger> logger;

    /// <summary>
    /// data plane logger for logging audit logs of data plane.
    /// </summary>
    private readonly ILogger auditDataPlaneLogger;

    /// <summary>
    /// control plane logger for logging audit logs of control plane.
    /// </summary>
    private readonly ILogger auditControlPlaneLogger;

    public AuditLogger(IHttpContextAccessor httpContextAccessor, ILogger<AuditLogger> logger, ILogger auditDataPlaneLogger, ILogger auditControlPlaneLogger)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
        this.auditDataPlaneLogger = auditDataPlaneLogger;
        this.auditControlPlaneLogger = auditControlPlaneLogger;
    }

    /// <summary>
    /// Logs success scenario for audit logging
    /// </summary>
    public void LogSuccess(string resource, string operationName, OperationType operationType, AuditLoggerType auditLoggerType = AuditLoggerType.DataPlane)
    {
        Log(OperationResult.Success, resource, operationName, operationType, auditLoggerType);
    }

    /// <summary>
    /// Logs failure scenario for audit logging
    /// </summary>
    public void LogFailure(string resource, string operationName, OperationType operationType, AuditLoggerType auditLoggerType = AuditLoggerType.DataPlane)
    {
        Log(OperationResult.Failure, resource, operationName, operationType, auditLoggerType);
    }

    /// <summary>
    /// Logs control and data plane records to geneva backend
    /// </summary>
    private void Log(OperationResult result, string resource, string operationName, OperationType operationType, AuditLoggerType auditLoggerType)
    {
        if (resource == null)
        {
            throw new ArgumentNullException(nameof(resource));
        }
        AuditRecord auditRecord = CreateAuditRecord(resource.ToString(), operationName, operationType, result);
        try
        {
            if (auditLoggerType == AuditLoggerType.ControlPlane)
            {
                auditControlPlaneLogger.LogAudit(auditRecord);
                logger.LogInformation($"Successfully pushed control plane audit record to geneva for operation {LogMessageTemplateConstants.EmptyPlaceholder}", operationName);
            }
            else
            {
                auditDataPlaneLogger.LogAudit(auditRecord);
                logger.LogInformation($"Successfully pushed data plane audit log to geneva for operation {LogMessageTemplateConstants.EmptyPlaceholder}", operationName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to push {LogMessageTemplateConstants.EmptyPlaceholder} audit record for operation {LogMessageTemplateConstants.EmptyPlaceholder} and trace is {LogMessageTemplateConstants.EmptyPlaceholder}", auditLoggerType.ToString(), operationName, ex.StackTrace);
        }
    }

    /// <summary>
    /// Create Audit record which is sent by the SDK to geneva backend
    /// </summary>
    private AuditRecord CreateAuditRecord(string resourceIdentifier, string operationName, OperationType operationType, OperationResult operationResult)
    {
        AuditRecord auditRecord = new()
        {
            OperationName = operationName,
            OperationType = operationType,
            OperationResult = operationResult,
            OperationAccessLevel = AuditRecordConstants.OperationAccessLevel,
            CallerAgent = AuditRecordConstants.CallerAgent
        };
        auditRecord.AddOperationCategory(OperationCategory.UserManagement);
        string appId = httpContextAccessor?.HttpContext?.User?.FindFirstValue("appId");
        if (!string.IsNullOrEmpty(appId))
        {
            auditRecord.AddCallerIdentity(CallerIdentityType.ApplicationID, appId);
        }

        auditRecord.AddCallerAccessLevel(AuditRecordConstants.CallerAccessLevel);
        auditRecord.AddTargetResource("Resource", resourceIdentifier);
        auditRecord.OperationResultDescription = (operationResult == OperationResult.Failure) ? AuditRecordConstants.OperationResultDescriptionValue : string.Empty;
        return auditRecord;
    }
}
