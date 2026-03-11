using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceHub.Framework;

namespace CodeNav.OutOfProc.Services;

public interface IOutOfProcService
{
    Task DoSomethingAsync(CancellationToken cancellationToken);

    public static class Configuration
    {
        public const string ServiceName = "OutOfProcComponent.OutOfProcService";
        public static readonly Version ServiceVersion = new(1, 0);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);

        public static ServiceRpcDescriptor ServiceDescriptor => new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);
    }
}
