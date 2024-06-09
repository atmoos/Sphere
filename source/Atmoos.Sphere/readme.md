# Atmoos.Sphere

A library that mainly contains mechanisms, often exposed as extension methods.

[![nuget package](https://img.shields.io/nuget/v/Atmoos.Sphere.svg?logo=nuget)](https://www.nuget.org/packages/Atmoos.Sphere)

## Async

### General Asynchronous Mechanisms

- Yielding back to a configurable synchronisation context using `ContextFlow`.
  - This can be useful to integrate components that have a hard dependency on a particular context.

### Mechanisms to Map "old-style" APIs to more modern APIs

- Ordering of an `IEnumerable<Task<T>>` by completion.
  - This ensures that the first task in the sequence is also the first to complete, potentially improving throughput.
- Wrap an event based asynchronous task that provides update events into an `IAsyncEnumerable<T>`.
  - This allows for an arguably more natural processing of the events as they arrive.

## Time

- Series of zero based time stamps
  - Both as synchronous and asynchronous versions.
- Generation of exponentially growing delays in an easy to consume asynchronous API
  - Exponential back-off comes to mind for retry mechanisms.

## Text

- Extension methods on `TextWriter` & `TextReader`.
- Extension methods to insert a given section of text into an existing larger piece of text.
  - It's inserted given a parametrisable line mark.
  - Supports:
    - `TextReader` & `TextWriter` pairs
    - Insertion into an existing file, given by a `FileInfo` instance.
  - In asynchronous and synchronous variants.

## Sync

Mechanisms that provide "[sync over async](https://devblogs.microsoft.com/pfxteam/should-i-expose-synchronous-wrappers-for-asynchronous-methods/)", i.e. avoid using this!

But if it is impossible to avoid, here you go:

- Use the `Await()` extension method to synchronously "await" any task.
  - Marked as obsolete, as it really should not be used.
  - It's much safer to refactor a codebase to use "async all the way".
