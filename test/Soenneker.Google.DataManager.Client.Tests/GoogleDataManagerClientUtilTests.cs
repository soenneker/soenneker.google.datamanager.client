using Google.Apis.Auth.OAuth2;
using Google.Apis.DataManager.v1;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Google.Credentials.Abstract;
using Soenneker.Google.DataManager.Client.Abstract;
using Soenneker.Google.DataManager.Client.Registrars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Google.DataManager.Client.Tests;

public sealed class GoogleDataManagerClientUtilTests
{
    [Test]
    public async ValueTask Cache_is_keyed_by_filename_and_requests_data_manager_scope(CancellationToken cancellationToken)
    {
        var credentials = new Credentials();
        await using var provider = new GoogleDataManagerClientUtil(credentials);
        DataManagerService[] results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => provider.Get("first.json", cancellationToken: cancellationToken).AsTask()));
        Check(results.All(service => ReferenceEquals(service, results[0])), "Concurrent gets created different services.");
        Check(credentials.Files.SequenceEqual(new[] { "first.json" }), "Credential loaded more than once.");
        Check(ReferenceEquals(results[0].HttpClientInitializer, credentials.Credential), "Shared credential not used.");
        Check(!ReferenceEquals(results[0], await provider.Get("second.json", cancellationToken: cancellationToken)), "Different files share a client.");
        Check(await provider.Remove("first.json", cancellationToken: cancellationToken), "Client not removed.");
        Check(!ReferenceEquals(results[0], await provider.Get("first.json", cancellationToken: cancellationToken)), "Removed client was reused.");
        provider.RemoveSync("second.json", cancellationToken: cancellationToken);
        await provider.Get("second.json", cancellationToken: cancellationToken);
        Check(credentials.Files.Count == 4 && !credentials.Disposed, "Invalid cache or credential ownership.");
    }

    [Test]
    public async ValueTask Scoped_registration_owns_separate_services(CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        services.AddScoped<IGoogleCredentialsUtil, Credentials>();
        services.AddGoogleDataManagerClientUtilAsScoped();
        await using var container = services.BuildServiceProvider();
        await using var first = container.CreateAsyncScope();
        await using var second = container.CreateAsyncScope();
        var a = first.ServiceProvider.GetRequiredService<IGoogleDataManagerClientUtil>();
        var b = second.ServiceProvider.GetRequiredService<IGoogleDataManagerClientUtil>();
        Check(!ReferenceEquals(await a.Get("same.json", cancellationToken: cancellationToken), await b.Get("same.json", cancellationToken: cancellationToken)), "Scopes share clients.");
        Check(!ReferenceEquals(first.ServiceProvider.GetRequiredService<IGoogleCredentialsUtil>(), second.ServiceProvider.GetRequiredService<IGoogleCredentialsUtil>()), "Scopes share credentials.");
    }

    [Test]
    public async ValueTask Get_after_disposal_is_rejected_without_disposing_shared_credentials(CancellationToken cancellationToken)
    {
        var credentials = new Credentials();
        var provider = new GoogleDataManagerClientUtil(credentials);
        await provider.Get("first.json", cancellationToken: cancellationToken);
        await provider.DisposeAsync();
        Check(!credentials.Disposed, "Provider disposed injected credentials.");
        try { await provider.Get("first.json", cancellationToken: cancellationToken); }
        catch (ObjectDisposedException) { return; }
        throw new Exception("Disposed provider returned a service.");
    }

    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    private sealed class Credentials : IGoogleCredentialsUtil
    {
        public ICredential Credential { get; } = GoogleCredential.FromAccessToken("offline-test-token").UnderlyingCredential;
        public List<string> Files { get; } = new();
        public bool Disposed { get; private set; }
        public ValueTask<ICredential> Get(string fileName, string[] scopes, CancellationToken cancellationToken = default)
        {
            Check(scopes.SequenceEqual(new[] { DataManagerService.Scope.Datamanager }), "Wrong OAuth scope.");
            Files.Add(fileName);
            return ValueTask.FromResult(Credential);
        }
        public ValueTask<bool> Remove(string fileName, string[] scopes, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void RemoveSync(string fileName, string[] scopes, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Dispose() => Disposed = true;
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
