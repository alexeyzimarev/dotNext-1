.NEXT
====
[![Build Status](https://dev.azure.com/dotnet/dotNext/_apis/build/status/dotnet.dotNext?branchName=master)](https://dev.azure.com/dotnet/dotNext/_build/latest?definitionId=1&branchName=master)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/dotnet/dotNext/blob/master/LICENSE)
![Test Coverage](https://img.shields.io/azure-devops/coverage/dotnet/dotnext/160/master)
[![CodeQL](https://github.com/dotnet/dotNext/workflows/CodeQL/badge.svg)](https://github.com/dotnet/dotNext/actions?query=workflow%3ACodeQL)
[![Join the chat](https://badges.gitter.im/dot_next/community.svg)](https://gitter.im/dot_next/community)

.NEXT (dotNext) is a set of powerful libraries aimed to improve development productivity and extend .NET API with unique features. Some of these features are planned in future releases of .NET platform but already implemented in the library:

| Proposal | Implementation |
| ---- | ---- |
| [Interop between function pointer and delegate](https://github.com/dotnet/csharplang/discussions/3680) | [DelegateHelpers](https://dotnet.github.io/dotNext/api/DotNext.DelegateHelpers.html) factory methods |
| [Check if an instance of T is default(T)](https://github.com/dotnet/corefx/issues/16209) | [IsDefault() method](https://dotnet.github.io/dotNext/api/DotNext.Runtime.Intrinsics.html) |
| [Concept Types](https://github.com/dotnet/csharplang/issues/110) | [Documentation](https://dotnet.github.io/dotNext/features/concept.html) |
| [Expression Trees covering additional language constructs](https://github.com/dotnet/csharplang/issues/158), i.e. `foreach`, `await`, patterns, multi-line lambda expressions | [Metaprogramming](https://dotnet.github.io/dotNext/features/metaprogramming/index.html) |
| [Async Locks](https://github.com/dotnet/corefx/issues/34073) | [Documentation](https://dotnet.github.io/dotNext/features/threading/index.html) |
| [High-performance general purpose Write-Ahead Log](https://github.com/dotnet/corefx/issues/25034) | [Persistent Log](https://dotnet.github.io/dotNext/features/cluster/wal.html)  |
| [Memory-mapped file as Memory&lt;byte&gt;](https://github.com/dotnet/runtime/issues/37227) | [MemoryMappedFileExtensions](https://dotnet.github.io/dotNext/features/io/mmfile.html) |
| [Memory-mapped file as ReadOnlySequence&lt;byte&gt;](https://github.com/dotnet/runtime/issues/24805) | [ReadOnlySequenceAccessor](https://dotnet.github.io/dotNext/api/DotNext.IO.MemoryMappedFiles.ReadOnlySequenceAccessor.html) |
| [A dictionary where the keys are represented by generic arguments](https://github.com/dotnet/runtime/issues/59718) | [Documentation](https://dotnet.github.io/dotNext/features/core/typem.html) |
| [Process asynchronous tasks as they complete](https://github.com/dotnet/runtime/issues/61959) | [Documentation](https://dotnet.github.io/dotNext/features/threading/taskpipe.html) |
| [Soft References](https://github.com/dotnet/runtime/issues/63113) | [Documentation](https://dotnet.github.io/dotNext/features/core/softref.html) |

Quick overview of additional features:

* [Attachment of user data to an arbitrary objects](https://dotnet.github.io/dotNext/features/core/userdata.html)
* Extended set of [atomic operations](https://dotnet.github.io/dotNext/features/core/atomic.html). Inspired by [AtomicInteger](https://docs.oracle.com/javase/10/docs/api/java/util/concurrent/atomic/AtomicInteger.html) and friends from Java
* [Fast Reflection](https://dotnet.github.io/dotNext/features/reflection/fast.html)
* Fast conversion of bytes to hexadecimal representation and vice versa using `ToHex` and `FromHex` methods from [Span](https://dotnet.github.io/dotNext/api/DotNext.Span.html) static class
* `ManualResetEvent`, `ReaderWriterLockSlim` and other synchronization primitives now have their [asynchronous versions](https://dotnet.github.io/dotNext/features/threading/rwlock.html)
* [Atomic](https://dotnet.github.io/dotNext/features/core/atomic.html) memory access operations for arbitrary value types including enums
* [PipeExtensions](https://dotnet.github.io/dotNext/api/DotNext.IO.Pipelines.PipeExtensions.html) provides high-level I/O operations for pipelines such as string encoding and decoding
* A rich set of high-performance [memory buffers](https://dotnet.github.io/dotNext/features/io/buffers.html) for efficient I/O
* String formatting, encoding and decoding with low GC pressure: [dynamic char buffers](https://dotnet.github.io/dotNext/features/io/buffers.html#char-buffer)
* Fully-featured [Raft implementation](https://github.com/dotnet/dotNext/tree/master/src/cluster)
* Fully-featured [HyParView implementation](https://github.com/dotnet/dotNext/tree/master/src/cluster)

All these things are implemented in 100% managed code on top of existing .NET API without modifications of Roslyn compiler or CoreFX libraries.

# Quick Links

* [Features](https://dotnet.github.io/dotNext/features/core/index.html)
* [API documentation](https://dotnet.github.io/dotNext/api.html)
* [Benchmarks](https://dotnet.github.io/dotNext/benchmarks.html)
* [NuGet Packages](https://www.nuget.org/profiles/rvsakno)

# What's new
Release Date: 06-12-2022

<a href="https://www.nuget.org/packages/dotnext/4.6.0">DotNext 4.6.0</a>
* Added `CharComparer` class that allows to compare single characters in the same way as [StringComparer](https://docs.microsoft.com/en-us/dotnet/api/system.stringcomparer) comparing strings
* Minor performance improvements of static methods declared in [Span](https://dotnet.github.io/dotNext/api/DotNext.Span.html) class
* Added stack manipulation methods to [BufferWriterSlim&lt;T&gt;](https://dotnet.github.io/dotNext/api/DotNext.Buffers.BufferWriterSlim-1.html) value type
* Introduced [Timeout.Expired](https://dotnet.github.io/dotNext/api/DotNext.Threading.Timeout.html) static property that allows to obtain expired timeout
* Added `LastOrNone` extension methods for various collection types
* Deprecated `DotNext.Runtime.CompilerServices.Shared<T>` value type

<a href="https://www.nuget.org/packages/dotnext.metaprogramming/4.6.0">DotNext.Metaprogramming 4.6.0</a>
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.reflection/4.6.0">DotNext.Reflection 4.6.0</a>
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.unsafe/4.6.0">DotNext.Unsafe 4.6.0</a>
* Small performance improvements of unmanaged memory allocator
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.threading/4.6.0">DotNext.Threading 4.6.0</a>
* Fixed incorrect array bounds check in [AsyncEventHub](https://dotnet.github.io/dotNext/api/DotNext.Threading.AsyncEventHub.html) class
* Optimized completion callback scheduling for all types of asynchronous locks
* Linked token created using `LinkedTokenSourceFactory.LinkTo` extension method now allows to track the originally canceled token
* Added `DotNext.Threading.Scheduler` static class that allows to delay execution of asynchronous tasks
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.io/4.6.0">DotNext.IO 4.6.0</a>
* Minor performance improvements of [FileReader](https://dotnet.github.io/dotNext/api/DotNext.IO.FileReader.html) data type
* Reduced memory allocation caused by asynchronous string decoding methods
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.net.cluster/4.6.0">DotNext.Net.Cluster 4.6.0</a>
* Optimized read barrier
* Fixed cancellation token propagation in public instance methods declared in [IRaftCluster](https://dotnet.github.io/dotNext/api/DotNext.Net.Cluster.Consensus.Raft.IRaftCluster.html) interface
* Introduced a simple framework for rumor spreading for peer-to-peer applications based on [Lamport timestamps](https://en.wikipedia.org/wiki/Lamport_timestamp): `DotNext.Net.Cluster.Messaging.Gossip.RumorTimestamp` and `DotNext.Net.Cluster.Messaging.Gossip.RumorSpreadingManager` classes. Also you can check out modified example of P2P application based on HyParView protocol in `src/examples` folder

<a href="https://www.nuget.org/packages/dotnext.aspnetcore.cluster/4.6.0">DotNext.AspNetCore.Cluster 4.6.0</a>
* Updated dependencies

Changelog for previous versions located [here](./CHANGELOG.md).

# Release & Support Policy
The libraries are versioned according with [Semantic Versioning 2.0](https://semver.org/).

| Version | .NET compatibility | Support Level |
| ---- | ---- | ---- |
| 0.x | .NET Standard 2.0 | Not Supported |
| 1.x | .NET Standard 2.0 | Not Supported |
| 2.x | .NET Standard 2.1 | Not Supported |
| 3.x | .NET Standard 2.1, .NET 5 | Not Supported |
| 4.x | .NET 6 | Active development |

_Maintenance_ support level means that new releases will contain bug fixes only.

# Development Process
Philosophy of development process:
1. All libraries in .NEXT family are available for the wide range of .NET runtimes: Mono, .NET, Blazor
1. Compatibility with R2R/AOT compiler should be checked for every release
1. Minimize set of dependencies
1. Provide high-quality documentation
1. Stay cross-platform
1. Provide benchmarks

# Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).
For more information see the [Code of Conduct FAQ](https://www.contributor-covenant.org/faq/) or
contact [conduct@dotnetfoundation.org](mailto:conduct@dotnetfoundation.org) with any additional questions or comments.