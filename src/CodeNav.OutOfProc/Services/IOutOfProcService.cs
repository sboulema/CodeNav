using Microsoft.ServiceHub.Framework;

namespace CodeNav.OutOfProc.Services;

public interface IOutOfProcService
{
    Task DoSomethingAsync(CancellationToken cancellationToken);

    Task SetCodeItemIsExpanded(int spanStart, int spanEnd, bool isExpanded);

    public static class Configuration
    {
        public const string ServiceName = "CodeNav.OutOfProcService";
        public static readonly Version ServiceVersion = new(1, 0);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);

        public static ServiceRpcDescriptor ServiceDescriptor => new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);
    }
}
