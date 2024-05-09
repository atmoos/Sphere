# Atmoos.Sphere

A library that mainly contains mechanisms, often exposed as extension methods.

## Async

Mechanisms that target asynchronous patterns.

### General Asynchronous Mechanisms

- Yielding back to a configurable synchronisation context using `ContextFlow`.
  - This can be useful to integrate components that have a hard dependency on a particular context.

### Mechanisms to Map "old-style" APIs to more modern APIs

- Ordering of an `IEnumerable<Task<T>>` by completion.
  - This ensures that the first task in the sequence is also the first to complete, potentially improving throughput.
- Wrap an event based asynchronous task that provides update events into an `IAsyncEnumerable<T>`.
  - This allows for an arguably more natural processing of the events as they arrive.

## Time

Mechanisms that target time based patterns.

- Generation of exponentially growing delays in an easy to consume asynchronous API
  - Exponential back-off comes to mind for retry mechanisms.

## Sync

Mechanisms that provide "[sync over async](https://devblogs.microsoft.com/pfxteam/should-i-expose-synchronous-wrappers-for-asynchronous-methods/)", i.e. avoid using this!

But if it is impossible to avoid, here you go:

- Use the `Await()` extension method to synchronously "await" any task.
  - Marked as obsolete, as it really should not be used.
  - It's much safer to refactor a codebase to use "async all the way".
