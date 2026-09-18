using Google.Apis.Auth.OAuth2;
using Google.Apis.DataManager.v1;
using Google.Apis.Services;
using Soenneker.Google.Credentials.Abstract;
using Soenneker.Google.DataManager.Client.Abstract;
using Soenneker.Dictionaries.SingletonKeys;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Google.DataManager.Client;

public sealed class GoogleDataManagerClientUtil : IGoogleDataManagerClientUtil
{
    private static readonly string[] _scopes = [DataManagerService.Scope.Datamanager];
    private readonly IGoogleCredentialsUtil _credentials;
    private readonly SingletonKeyDictionary<string, DataManagerService> _services;

    public GoogleDataManagerClientUtil(IGoogleCredentialsUtil credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        _credentials = credentials;
        _services = new SingletonKeyDictionary<string, DataManagerService>(CreateService);
    }

    private async ValueTask<DataManagerService> CreateService(string fileName, CancellationToken cancellationToken)
    {
        ICredential credential = await _credentials.Get(fileName, _scopes, cancellationToken).ConfigureAwait(false);
        return new DataManagerService(new BaseClientService.Initializer { HttpClientInitializer = credential });
    }

    public ValueTask<DataManagerService> Get(string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return _services.Get(fileName, cancellationToken);
    }

    public ValueTask<bool> Remove(string fileName, CancellationToken cancellationToken = default) => _services.Remove(fileName, cancellationToken);
    public void RemoveSync(string fileName, CancellationToken cancellationToken = default) => _services.RemoveSync(fileName, cancellationToken);
    public void Dispose() => _services.Dispose();
    public ValueTask DisposeAsync() => _services.DisposeAsync();
}
