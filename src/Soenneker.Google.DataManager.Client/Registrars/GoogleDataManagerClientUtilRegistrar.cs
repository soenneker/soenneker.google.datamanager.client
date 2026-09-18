using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Google.Credentials.Registrars;
using Soenneker.Google.DataManager.Client.Abstract;

namespace Soenneker.Google.DataManager.Client.Registrars;

/// <summary>Registers Data Manager clients and their service-account credential cache.</summary>
public static class GoogleDataManagerClientUtilRegistrar
{
    /// <summary>Registers the client provider and credential cache as singletons.</summary>
    public static IServiceCollection AddGoogleDataManagerClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGoogleCredentialsUtilAsSingleton().TryAddSingleton<IGoogleDataManagerClientUtil, GoogleDataManagerClientUtil>();
        return services;
    }

    /// <summary>Registers the client provider and credential cache per scope.</summary>
    public static IServiceCollection AddGoogleDataManagerClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddGoogleCredentialsUtilAsScoped().TryAddScoped<IGoogleDataManagerClientUtil, GoogleDataManagerClientUtil>();
        return services;
    }
}
