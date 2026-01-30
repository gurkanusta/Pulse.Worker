# Pulse.Worker

A .NET 8 background worker for asynchronous email processing.

## What it does
- Processes email jobs in the background using `BackgroundService`
- Uses an in-memory queue (`System.Threading.Channels`)
- Retries failed jobs with Polly
- Moves permanently failed jobs to a Dead Letter Queue
- Structured logging with Serilog and CorrelationId

## Tech Stack
- .NET 8
- BackgroundService
- Channels
- Polly
- Serilog


