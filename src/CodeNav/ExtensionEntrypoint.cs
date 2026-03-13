using CodeNav.Languages.CSharp.Mappers;
using CodeNav.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav
{
    /// <summary>
    /// Extension entrypoint for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension
    {
        /// <inheritdoc />
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            RequiresInProcessHosting = true,
            LoadedWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, "csharp"),
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            // Must be singleton so we have a single source of truth for the code document
            serviceCollection.AddSingleton<CodeDocumentService>();

            base.InitializeServices(serviceCollection);

            // You can configure dependency injection here by adding services to the serviceCollection.

            serviceCollection.AddScoped<TextViewService>();

            serviceCollection.AddScoped<BaseMapper>();
            serviceCollection.AddScoped<ClassMapper>();
            serviceCollection.AddScoped<DelegateEventMapper>();
            serviceCollection.AddScoped<FieldMapper>();
            serviceCollection.AddScoped<IndexerMapper>();
            serviceCollection.AddScoped<InterfaceMapper>();
            serviceCollection.AddScoped<MethodMapper>();
            serviceCollection.AddScoped<NamespaceMapper>();
            serviceCollection.AddScoped<PropertyMapper>();
            serviceCollection.AddScoped<RecordMapper>();
            serviceCollection.AddScoped<StatementMapper>();
            serviceCollection.AddScoped<StructMapper>();
        }
    }
}
