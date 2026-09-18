# Soenneker.Google.DataManager.Client

A lazy, thread-safe Data Manager service provider using `IGoogleCredentialsUtil`, matching the Google Indexing Service library. Targets .NET 10.

## Registration and use

```csharp
using Soenneker.Google.DataManager.Client.Registrars;

services.AddGoogleDataManagerClientUtilAsSingleton();
```

Registration also adds the shared credential utility. `AddGoogleDataManagerClientUtilAsScoped()` registers both the client provider and credential utility per scope.

Place your service-account JSON beneath the application's `LocalResources` directory and ensure your application copies it to its output directory. Keep the credential out of source control and packages. Enable the Data Manager API and grant the service account access to the destination advertising account.

```csharp
using Google.Apis.DataManager.v1;
using Soenneker.Google.DataManager.Client.Abstract;

public sealed class Example(IGoogleDataManagerClientUtil provider)
{
    public async Task UseClient(CancellationToken cancellationToken)
    {
        DataManagerService service = await provider.Get("google-sales.json", cancellationToken);
        // Execute requests through service.Events or service.RequestStatus.
        // The provider owns this service; do not dispose it here.
    }
}
```

`Get(fileName)` passes the filename and `https://www.googleapis.com/auth/datamanager` scope to `IGoogleCredentialsUtil`. Credentials are loaded from LocalResources and cached by filename/scopes. Data Manager clients are separately cached by filename, so concurrent calls for the same file share a service and different files get different services.

`Remove(fileName)` and `RemoveSync(fileName)` remove and dispose a cached client. They do not invalidate the credential utility's separate cache. For credential rotation, remove the matching filename/scope entry from `IGoogleCredentialsUtil` as well before recreating the client. Finish in-flight requests before removing clients or disposing the provider. The provider does not dispose injected credentials; DI manages their lifetime.

## Build and verify

```powershell
dotnet build
dotnet test --project test/Soenneker.Google.DataManager.Client.Tests -- --treenode-filter "/*/*/GoogleDataManagerClientUtilTests/*"
```

Tests use a fake credential provider and require no Google account.
