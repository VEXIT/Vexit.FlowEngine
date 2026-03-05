
|              |                                                                                                                             |
| ------------ | --------------------------------------------------------------------------------------------------------------------------- |
| Copyright    | © VEXIT ® 2026 , www.vexit.com , Tomorrow is today...                                                                       |
| Author       | Vex Tatarevic                                                                                                               |
| Date Created | 2026-01-16                                                                                                                  |
| Date Updated | 2026-01-17 - Vex - Developed  final draft of project architecture. Explained what each file does and how they fit together. |
|              | 2026-01-28 - Vex - Updated flow constants structure to per-feature Flows.cs files                                           |
|              | 2026-01-31 - Vex - Updated flow storage configuration defaults.                                                       |
|              | 2026-02-06 - Vex - Added IFlowObserver documentation.                                                                       |



## Project Structure

```
Vexit.FlowEngine/          # 📚 Library (generic, reusable)
├── Core/                  # Contracts + shared metadata
│   |
│   ├── Abstractions/              # Public interfaces
│   │   ├── IFlowRunner.cs                 # Public runner (code-first + JSON)
│   │   ├── IFlow<TContext>.cs             # Orchestration contract (code-first)
│   │   ├── IActivity.cs                   # Activity contract (units of work)
│   │   ├── IFlowController.cs             # In-flow control surface (execute/branch/state)
│   │   ├── IActivityRegistry.cs           # Handler registry contract (JSON flows)
│   │   ├── IStateStore.cs                 # Persistence abstraction (resume/audit)
│   │   └── IInputResolver.cs              # Input binding resolver ($ref, context)
│   |
│   └── Attributes/                # Metadata for reflection
│       ├── ActivityAttribute.cs           # Base activity attribute + Step/Group/Decision specializations
│       ├── FlowServiceAttribute.cs         # Declares services to scan for activities
│       ├── FlowAttribute.cs               # Flow metadata (id, version, description)
│       └── InputAttribute.cs              # Defines parameter bindings
│
├── Discovery/             # Metadata discovery
│   |
│   ├── Scanners/                  # Reflection scanners
│   │   ├── ActivityScanner.cs             # Discovers activity methods/classes
│   │   └── FlowScanner.cs                 # Discovers flows/metadata
│   |
│   └── Registry/                  # Metadata caches
│       ├── ActivityRegistry.cs            # Activity id -> handler mapping
│       └── FlowRegistry.cs                # Flow id -> metadata mapping
│
├── Enums/                 # Type-safe enumerations
│   └── ActivityTypeSE.cs          # Activity types (Step/Group/Decision)
│
├── Hosting/               # DI setup (CliEngine-style)
|    ├── FlowEngineExtensions.cs    # AddFlowEngine(...) extension
|    └── FlowEngineOptions.cs       # Defaults to entry assembly + slice discovery
│
├── Models/                # DTOs (definitions + runtime state)
│   ├── ActivityDefinition.cs      # Per-step config
│   ├── ActivityInstance.cs        # Step run history
│   ├── FlowDefinition.cs          # JSON blueprint (steps/transitions)
│   ├── FlowInstance.cs            # Persisted execution state
│   ├── InputBinding.cs            # Input resolution rules
│   └── Transition.cs              # Step transition logic
│
└── Runtime/               # Engine implementation
    ├── FlowOrchestrator.cs        # Concrete runner/controller implementation
    ├── InputResolver.cs           # Binding resolution implementation
    └── StateStore.cs              # File/DB persistence impl

Vexit.VxServerCli/         # 🛠️ Consumer (domain-specific, vertical slices)
├── Commands/
│   ├── Server/
│   │   ├── Flows.cs                 # 🎯 Flow IDs for server operations
│   │   └── Setup/
│   │       ├── SetupCmd.cs                 # CLI entrypoint (command)
│   │       ├── Flow/
│   │       │   ├── ServerSetupFlow.cs      # IFlow<ServerSetupContext> (code-first)
│   │       │   └── ServerSetupContext.cs   # Flow state (strongly typed)
│   │       ├── Services/                   # Activity methods (no class-per-step)
│   │       │   └── ServerSetupService.cs   # Step methods return Result<Dictionary<string, object>>
│   │       └── Models/                     # Inputs/outputs specific to setup
│   └── Domain/
│       ├── Flows.cs                 # 🎯 Flow IDs for domain operations
│       └── Setup/
│   └── Mail/
│       ├── Flows.cs                 # 🎯 Flow IDs for mail operations
│       └── Setup/
├── Constants/                      # Shared across commands (Cmd.cs)
├── Ops/                            # Shared operations
├── Services/                       # Cross-cutting services (SSH, config)
├── Utils/                          # Shared utilities
└── Program.cs                      # Registry setup + DI
```

## Core Types Usage

| Role        | Name             | Purpose                                                                  |
| ----------- | ---------------- | ------------------------------------------------------------------------ |
| Public API  | IFlowRunner      | Injected into CLI commands. Used to start or resume a flow.              |
| Logic API   | IFlowController  | Passed into workflow logic. Used to drive steps and groups.              |
| Engine Room | FlowOrchestrator | Concrete implementation of both. Manages registry, state, and execution. |

How it looks in action - code example for cli app VxServerCli , command `vxs server setup`:

1. The Consumer (CLI Command):

   ```csharp
   // Uses the Runner interface
   public async Task ExecuteAsync() {
       await _flowRunner.RunAsync<ServerSetupFlow>(Flows.Server.Setup.Id, context);
   }
   ```

2. The Logic (Workflow):

   ```csharp
   // Uses the Controller interface
   public class ServerSetupFlow : IFlow<ServerSetupContext> {
       public async Task Build(IFlowController flow, ServerSetupContext ctx) {
          await flow.Step(Flows.ServerSetup.ConnectInitial, new { ctx.InitialUser, ctx.IP });
       }
   }
   ```

3. The Library (The Engine Room):

   ```csharp
   // The big class that implements both roles
   public class FlowOrchestrator : IFlowRunner, IFlowController {
       // Implements RunAsync (IFlowRunner)
       // Implements Step(), Group(), Decide() (IFlowController)
       // Handles Registry, StateStore, and Logging internally
   }
   ```
 
Notes on core types:
- IFlow<TContext> defines the orchestration contract (the sequence/logic), not the runtime.
- IStateStore persists FlowInstance state for resume, audit, and failure recovery.
- context in RunAsync<ServerSetupFlow>(instanceId, context) is the runtime input/state object.
- ServerSetupContext is a plain POCO (optionally implements a small marker interface) that carries inputs like host alias, flags, and derived state.

## Metadata Driven Discovery Usage

- `AddFlowEngine()` defaults to entry assembly scanning (automatic discovery).
- Slice discovery: flows/activities are found in the same namespace slice as IFlow<TContext>.
- `[FlowService<T>]` declares the main service containing all activity methods for this flow.
- The service can inject other dependencies internally via normal DI; FlowService just identifies where to find activities.
- Scanners populate registries; FlowOrchestrator consumes registries only.
- Input metadata doubles as tool manifest (AI-friendly descriptions).


| Strategy            | How it works                                                                                                                               |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Near-Zero Wiring   | `AddFlowEngine()` handles discovery automatically, but flows need manual DI registration. See "Flow Registration" section.                 |
| Slice Discovery     | By looking for classes in the same namespace as your `IFlow<T>`, you ensure that vertical slices (like Server/Setup/) stay self-contained. |
| Service Declaration | Using `[FlowService<ServerSetupService>]` declares the main service containing all activity methods for this flow.                         |
| Scanner Discovery   | Scanners (ActivityScanner.cs, FlowScanner.cs) automatically discover activities and flows in the current assembly.                         |
| Registry Caching    | Registries (ActivityRegistry.cs, FlowRegistry.cs) cache discovered metadata for fast lookup.                                               |
| Input Metadata      | Input metadata (InputAttribute.cs) doubles as tool manifest (AI-friendly descriptions).                                                    |


## FlowObserver

Optional telemetry interface for flow execution events. Implement `IFlowObserver` to handle logging, UI feedback, or metrics during flow runs.

**Register observer:**
```csharp
services.AddSingleton<IFlowObserver, MyObserver>();
```

**Implement observer:**
```csharp
public class MyObserver : IFlowObserver
{
    public void OnFlowStarted(string flowId, Guid instanceId, int totalSteps)
    {
        Console.WriteLine($"Flow {flowId} started with {totalSteps} steps");
    }

    public void OnActivityStarted(ActivityInfo activityInfo, ActivityState activityState, FlowProgress progress)
    {
        Console.WriteLine($"Running: {activityInfo.Name} ({progress.CompletedSteps + 1}/{progress.TotalSteps})");
    }

    public void OnActivityCompleted(ActivityInfo activityInfo, Result<ActivityState> result, FlowProgress progress)
    {
        var status = result.IsSuccess ? "✓" : "✗";
        Console.WriteLine($"{activityInfo.Name} {status}");
    }

    // ... other methods
}
```

Observer failures don't break flows. Register multiple observers for different concerns.

Logic Interaction Example (Agent-Ready Workflow):

```csharp
// Inside ServerSetupService.cs
[Step(Activities.Ssh.Connect)]
[Input("ip", "Target server IP")]
public async Task<Result> Connect(string ip) { ... }
```

End-to-end minimal wiring (vxs server setup):

```csharp
// Program.cs
builder.AddFlowEngine(); // handles discovery: entry assembly + slice scanning
```

## Flow Registration

⚠️ **Important:** While `AddFlowEngine()` handles automatic discovery of flows and activities, **flows must be manually registered in DI** for proper instantiation.

### Automatic Discovery
- `FlowScanner` finds classes implementing `IFlow<TContext>`
- `ActivityScanner` finds methods with activity attributes
- Metadata is registered in `FlowRegistry` and `ActivityRegistry`

### Manual DI Registration Required
Flows often have constructor dependencies and must be registered as services:

```csharp
// In Program.cs
var builder = Host.CreateApplicationBuilder(args);

// Register flows for DI resolution
builder.Services.AddTransient<ServerSetupFlow>();
builder.Services.AddTransient<MyOtherFlow>();

// Register activity services
builder.Services.AddTransient<ServerSetupService>();

// Add FlowEngine (handles discovery)
builder.Services.AddVexitFlowEngine();

var app = builder.Build();
```

**Why?** `FlowOrchestrator.CreateFlow<TFlow>()` tries DI resolution first:
```csharp
private TFlow CreateFlow<TFlow>()
{
    var flow = _serviceProvider.GetService(typeof(TFlow)) as TFlow;
    return flow ?? Activator.CreateInstance<TFlow>(); // Fallback
}
```

Without DI registration, flows fall back to parameterless constructors, which fail if dependencies are needed.

```csharp
// Commands/Server/Setup/Flow/ServerSetupFlow.cs
[Flow(Flows.Server.Setup.Id)]
[FlowService<ServerSetupService>]
public class ServerSetupFlow : IFlow<ServerSetupContext>
{
    public async Task Build(IFlowController flow, ServerSetupContext ctx)
    {
        // Phase 1: Initialization
        await flow.Group("Initialization", async () =>
        {
            await flow.Step(Flows.Server.Setup.ConnectInitial, new { ctx.InitialUser, ctx.IP });
            await flow.Step(Flows.Server.Setup.GenerateSshKeys);
            await flow.Step(Flows.Server.Setup.SetupAdminUser);
            await flow.Step(Flows.Server.Setup.UpdateLocalSshConfig);
        });

        // Phase 2: Provisioning
        await flow.Group("Provisioning", async () =>
        {
            await flow.Step(Flows.Server.Setup.ConnectAdmin);
            await flow.Step(Flows.Server.Setup.SetupFirewall);
            await flow.Step(Flows.Server.Setup.InstallStack);
        });
    }
}
```

```csharp
// Commands/Server/Flows.cs - Flow ID constants
public static class Flows
{
    public static class Server
    {
        public const string Name = "server";
        public static string Path => Name;

        public static class Setup
        {
            public const string Name = "setup";
            public static string Path => $"{Server.Path}.{Name}";

            // Core system setup
            public static string UfwFirewall => $"{Path}.step.ufw_firewall";
            public static string SystemHostname => $"{Path}.step.system_hostname";
            public static string AdminShell => $"{Path}.step.admin_shell";

            // Software installation
            public static string SystemUpdate => $"{Path}.step.system_update";
            public static string Dotnet => $"{Path}.step.dotnet";
            public static string Node => $"{Path}.step.node";
            public static string Postgres => $"{Path}.step.postgres";
            public static string Nginx => $"{Path}.step.nginx";
            public static string MailServer => $"{Path}.step.mail_server";
            public static string Certbot => $"{Path}.step.certbot";

            // Service configuration
            public static string BasicNginxConfig => $"{Path}.step.basic_nginx_config";
            public static string WelcomePage => $"{Path}.step.welcome_page";
            public static string RestartServices => $"{Path}.step.restart_services";
        }
    }
}

// Commands/Server/Setup/Services/ServerSetupService.cs
public class ServerSetupService
{
    [Step(Flows.Server.Setup.ConnectInitial)]
    [Input("initialUser", "Initial user for connection")]
    [Input("ip", "Target server IP")]
    public async Task<Result> ConnectInitial(string initialUser, string ip) { ... }

    [Step(Flows.Server.Setup.GenerateSshKeys)]
    public async Task<Result> GenerateSshKeys() { ... }

    [Step(Flows.Server.Setup.SetupAdminUser)]
    public async Task<Result> SetupAdminUser() { ... }

    [Step(Flows.Server.Setup.UpdateLocalSshConfig)]
    public async Task<Result> UpdateLocalSshConfig() { ... }

    [Step(Flows.Server.Setup.ConnectAdmin)]
    public async Task<Result> ConnectAdmin() { ... }

    [Step(Flows.Server.Setup.SetupFirewall)]
    public async Task<Result> SetupFirewall() { ... }

    [Step(Flows.Server.Setup.InstallStack)]
    public async Task<Result> InstallStack() { ... }
}
```

```csharp
// Commands/Server/Setup/SetupCmd.cs
public class SetupCmd : CmdBase
{
    private readonly IFlowRunner _flowRunner;

    public SetupCmd(IFlowRunner flowRunner) => _flowRunner = flowRunner;

    public override async Task<Result> ExecuteAsync()
    {
        var context = new ServerSetupContext { HostAlias = HostAlias };
        return await _flowRunner.RunAsync<ServerSetupFlow>(Flows.Server.Setup.Id, context);
    }
}
```

## Configuration and Storage

FlowEngine provides flexible configuration options at both global and per-flow levels, allowing fine-grained control over discovery, execution, and persistence.

### Global Configuration (FlowEngineOptions)

Set at application startup (Program.cs) via `AddVexitFlowEngine()`:

```csharp
builder.AddVexitFlowEngine(options =>
{
    options.StateStoreType = StateStoreTypeSE.File;        // File or Database
    options.StateStoreLocation = StateStoreLocationSE.Local; // Local or Remote
    options.StateStoreBasePath = "~/.vexit/flows";         // Base storage path
    options.DiscoveryAssembly = Assembly.GetEntryAssembly(); // Assembly to scan
});
```

### Storage Configuration

FlowEngine uses a single, global storage configuration set via `AddVexitFlowEngine()`. The instance ID is passed directly to `RunAsync()` and used to name the persisted state file.

---

## Data-First Flow Models (Flat Design)

Unlike old polymorphic designs with inheritance hierarchies, our **flat data-first approach** uses simple POCO classes that are perfect for JSON storage and AI processing.

### InputBinding: How Inputs Are Resolved
`InputBinding` tells the engine where to get input values from:

```csharp
public class InputBinding
{
    public string Source { get; set; }  // "context", "literal", "step"
    public string Key { get; set; }     // lookup key/path (e.g. "Server.Ip" or "16")
    public object? Value { get; set; }  // literal value when Source="literal"
}
```

**Examples:**
- From flow context: `{"source": "context", "key": "Server.Ip"}` → gets `ctx.Server.Ip`
- Literal value: `{"source": "literal", "value": "16"}` → passes `16`
- From step output: `{"source": "step", "key": "generate.output.password"}` → gets previous step result

### Transition: Flow Control Logic
`Transition` defines a single rule for what happens after each step. Multiple transitions allow complex branching:

```csharp
public class Transition
{
    public string When { get; set; }        // result to match ("Success", "Failed", "Retry")
    public string To { get; set; }          // next step ID
    public string? Condition { get; set; }  // optional condition expression
}
```

**Examples:**
- Simple next: `[{"when": "Success", "to": "install-postgres"}]`
- Error handling: `[{"when": "Success", "to": "install-nginx"}, {"when": "Failed", "to": "rollback"}]`
- Decisions: `[{"when": "Approved", "to": "deploy"}, {"when": "Denied", "to": "cleanup"}]`

### Why Flat Design Wins
**Old Polymorphic Way:**
```csharp
public abstract class Activity { }
public class Step : Activity { public string Handler { get; set; } }
public class Decision : Activity { public Dictionary<string, string> Branches { get; set; } }
```
- Hard to serialize to JSON
- Inheritance adds complexity
- AI can't easily generate or modify

**New Flat Way:**
```json
{
  "id": "check-status",
  "type": "Step",
  "handler": "CheckServiceStatus",
  "transitions": [
    {"when": "Success", "to": "log-success"},
    {"when": "Failed", "to": "send-alert"}
  ]
}
```
- Pure data, easy to store/load
- AI can generate/modify with JSON
- Simple to validate and debug
- No inheritance overhead



---

*© VEXIT ® 2026 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*