using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Yandex.Cloud.Operation;

namespace Yandex.Cloud;

public static class SdkExtensions
{
    public static Task<Operation.Operation> WaitForCompletionAsync(this Sdk sdk, Operation.Operation operation,
        CancellationToken cancellationToken = default)
    {
        return sdk.WaitForCompletionAsync(operation, TimeSpan.FromSeconds(1), cancellationToken);
    }
        
    public static async Task<Operation.Operation> WaitForCompletionAsync(this Sdk sdk, Operation.Operation operation, TimeSpan pollingInterval, CancellationToken cancellationToken = default)
    {
        if (operation.Done)
        {
            return operation;
        }

        var operationId = operation.Id;
        var retryableCount = 0;
        while (true)
        {
            try
            {
                operation = await sdk.Services.Operation.OperationService.GetAsync(
                    new GetOperationRequest
                    {
                        OperationId = operationId
                    },
                    cancellationToken: cancellationToken);

                if (operation.Done)
                {
                    return operation;
                }
            }
            catch (RpcException ex) when(ex.StatusCode == StatusCode.NotFound)
            {
                retryableCount++;
                if (retryableCount >= 3)
                {
                    throw;
                }
            }
                
            await Task.Delay(pollingInterval, cancellationToken);
        }
    }
        
    public static Task<Operation.Operation> WaitForCompletionAsync(this Operation.Operation operation, Sdk sdk,
        CancellationToken cancellationToken = default)
    {
        return sdk.WaitForCompletionAsync(operation, TimeSpan.FromSeconds(1), cancellationToken);
    }
        
    public static async Task<Operation.Operation> WaitForCompletionAsync(this AsyncUnaryCall<Operation.Operation> call, Sdk sdk,
        CancellationToken cancellationToken = default)
    {
        var operation = await call.ResponseAsync;
        return await sdk.WaitForCompletionAsync(operation, TimeSpan.FromSeconds(1), cancellationToken);
    }
}