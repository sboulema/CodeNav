using Microsoft.ServiceHub.Framework;

namespace CodeNav.Services;

public interface IInProcService
{
    Task DoSomethingAsync(CancellationToken cancellationToken);

    Task TextViewScrollToSpan(int start, int length);

    Task ExpandOutlineRegion(int start, int length);

    Task CollapseOutlineRegion(int start, int length);

    Task SubscribeToRegionEvents();

    public static class Configuration
    {
        public const string ServiceName = "CodeNav.InProcService";
        public static readonly Version ServiceVersion = new(1, 0);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);

        public static ServiceRpcDescriptor ServiceDescriptor => new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);
    }
}
