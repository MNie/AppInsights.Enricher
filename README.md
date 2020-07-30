# AppInsights.Enricher [![NuGet](https://buildstats.info/nuget/AppInsights.Enricher?includePreReleases=true)](https://www.nuget.org/packages/AppInsights.Enricher)


Could be downloaded from NuGet:
- ```Install-Package AppInsights.Enricher```
- ```dotnet add package AppInsights.Enricher```
- ```paket add AppInsights.Enricher```

How to use it:

* implement own ITelemetryEnricher
You say here when you want to store Request/Response:

```csharp
public class CustomTelemetryEnricher : ITelemetryEnricher
{
    public ITelemetry Enrich(ITelemetry tele) => tele;
    public bool AttachRequest(HttpContext context) => true;
    public bool AttachResponse(HttpContext context) => context?.Request?.Path.Value?.Contains("mail") == true;
}
```

* implement own IProcessorApplier
You say here when you want to log data to AppInsights

```csharp
public class CustomProcessorApplier: IProcessorApplier
{
    public bool Apply(ITelemetry tele)
    {
        if (tele is RequestTelemetry request)
        {
            var isItBadRequestOrConnection = request.ResponseCode == "404";
            if (isItBadRequestOrConnection)
                return false;
        }

        if (!(tele is DependencyTelemetry dependency)) return true;

        return true;
    }
}
```

* Register it in container

```csharp
services.AddApplicationInsightsKubernetesEnricher();
services.AddSingleton<IProcessorApplier, CustomProcessorApplier>();
services.AddSingleton<ITelemetryEnricher, CustomTelemetryEnricher>();
services.AddSingleton<IRequestDataAccessor>(new RequestDataAccessor(1_000, 1_000, 100_000));
return services.AddTelemetry<TelemetryProcessor>(new ApplicationInsightsServiceOptions { InstrumentationKey = "your_key" });
```

And that's all!
