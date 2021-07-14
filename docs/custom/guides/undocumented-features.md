Title: Using Undocumented Features
----------------------------------

The unfortunate reality of using the Discord API is that, while the online 
documentation is extensive, it is not exhaustive. Discord often exposes 
undocumented fields, endpoints, and data structures that may still be of use to
developers, even though they aren't ready, finished, or thoroughly documented.

Remora has a few ways for you to access these undocumented features, but beware!
As with any unstable API surface, things may break from one second to the next
without warning.

## Data
### Undocumented Fields
If you want to implement access to an undocumented field on an existing type,
create a new record that inherits from the existing model, and register it with
the DI system.

```cs
public record SomeExistingDataWithMoreStuff(int Existing, int Additional) 
    : SomeExistingData(Existing);
```

Note that to *override* an existing data model, you have to register it *after*
Remora's own setup.

```cs
// Add the base types from Remora
var serviceCollection = new ServiceCollection()
    .AddDiscordGateway(_ => botToken)
    .AddDiscordCommands(true);

// Add overriding data models
serviceCollection.Configure<JsonSerializerOptions>
(
    options => 
    {
        options.AddDataObjectConverter<ISomeExistingData, SomeExistingDataWithMoreStuff>();
    }
);
```

You can also specialize various parts of the data model when registering it, 
such as property names (which is useful for C#-ifying naming of boolean 
properties) and their type converters.

### Undocumented Data Structures
Any undocumented data structure can be added to Remora externally, and treated
as if it were bundled with Remora using the existing DI system.

Suppose we have the following undocumented data:

```json
{
  "some_name": 10
}
```

You would then model and register the following structures.

```cs
public interface ISomeData
{
    int SomeName { get; }
}

public record SomeData(int SomeName) : ISomeData;
```

Registration is the same as if you were implementing an undocumented field, but 
you may register it before Remora's own types.

## Gateway
### Undocumented Events & Commands
Any undocumented event or command can be added to Remora the same way as you 
would add a new undocumented data structure. However, you only need to model the
data portion of the event or command.

Suppose we have the following undocumented event:

```json
{
  "t": "SOME_UNDOCUMENTED_EVENT",
  "s": 4,
  "op": 0,
  "d": {
      "some_name": 10
  }
}
```

You would then model and register the following event data. Note that event 
names *must* match the interface name you create - that is, if the event is 
named `SOME_EVENT`, your interface must be named `ISomeEvent`.

```cs
public interface ISomeUndocumentedEvent
{
    int SomeName { get; }
}

public record SomeUndocumentedEvent(int SomeName) : ISomeUndocumentedEvent;
```

## REST
Features that relate to the REST API is customized through one of two methods.

The first is the `DiscordHttpClient` type, which can be accessed and used 
through DI. It's a named, transient `HttpClient`, which takes care of minutia 
like authorization headers and respecting rate limits for you. You would mainly
use this type when you want to make requests to completely undocumented
endpoints, or to take complete control over a call to a known endpoint.

```cs
public class Somewhere
{
    private readonly DiscordHttpClient _client;

    public Somewhere(DiscordHttpClient client)
    {
        _client = client;
    }
}
```

The second is through the concrete implementations of the API interfaces, which
allow you to perform smaller tweaks or changes to existing API methods, such as
adding headers or JSON payload fields.

### Undocumented Endpoints
To access an undocumented endpoint, you may use the `DiscordHttpClient` directly
to make any kind of HTTP request, similar to how you might use a normal 
`HttpClient`. All requests made by this client are prepended with the most 
recently versioned Discord API base endpoint, which typically looks like this: 
`https://discord.com/api/v9/`. Therefore, you should only use relative endpoints
when making requests.

```cs
var result = await _client.GetAsync<ISomeData>($"somewhere/{someId}/data");
```

Refer to the existing implementations of endpoints in the library for examples
of how to add JSON parameters, HTTP headers, or similar data.

### Undocumented Parameters & Headers
If you want to provide additional data to an existing endpoint, such as JSON
parameters, HTTP headers, or query string parameters, you can easily add
customizations to *all* requests made by the API within a particular scope. This
feature is only available to you when you inject the concrete implementation of 
an API category, however, and not through the interface.

This means that if you want to, for example, send an additional JSON field when
sending a message, you would need to inject `DiscordRestChannelAPI` and not 
`IDiscordRestChannelAPI`.

This is useful when an endpoint is discovered to allow undocumented parameters,
or metadata headers such as `X-Audit-Log-Reason`.

Once you have the concrete implementation available to you, creating a 
customization is simple.
```cs
using (_ = _api.WithCustomization(r => r.WithJson(json => json.WriteString("name", "value"))))
{
    // This call will now have "name": "value" in its JSON payload, in addition
    // to the normal data.
    var result = await _api.SomeEndpointAsync();
}

// This call will not have any additional data
var result = await _api.SomeEndpointAsync();
```

The customization uses the same types and logic for configuring a request's 
parameters, methods, and headers as implementing a custom endpoint, so if you 
learn one you should have no trouble using the other.

Multiple customizations may be in effect at the same time, and will be applied 
in the order you create them. A customization is removed from the client when it
is disposed.
