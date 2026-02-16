using Polly;
using Polly.Retry;
using Serilog;
using System.Threading.Channels;
using Pulse.Worker.Email;
using Pulse.Worker.Models;
using Pulse.Worker.Options;
using Pulse.Worker.Queue;
using Pulse.Worker.Workers;

using Pulse.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPulseWorker();

var host = builder.Build();
await host.RunAsync();
