using Google.Apis.DataManager.v1;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Google.DataManager.Client.Abstract;

/// <summary>Provides lazily initialized Data Manager clients keyed by service-account credential filename.</summary>
public interface IGoogleDataManagerClientUtil : IDisposable, IAsyncDisposable
{
    /// <summary>Gets or creates a client using a credential filename relative to LocalResources and the Data Manager scope.
    /// The provider owns the client; callers must not dispose it. Complete in-flight requests before removing or disposing clients.</summary>
    ValueTask<DataManagerService> Get(string fileName, CancellationToken cancellationToken = default);

    /// <summary>Removes and disposes the cached client for a filename. Does not invalidate the separate credential cache.</summary>
    ValueTask<bool> Remove(string fileName, CancellationToken cancellationToken = default);

    /// <summary>Synchronously removes and disposes the cached client. Does not invalidate the separate credential cache.</summary>
    void RemoveSync(string fileName, CancellationToken cancellationToken = default);
}
