using System.Buffers;
using System.CommandLine;
using System.CommandLine.IO;

namespace DotNext.Maintenance.CommandLine.IO;

using Buffers;

internal sealed class MaintenanceConsole : Disposable, IMaintenanceConsole
{
    private sealed class BufferedStreamWriter : Disposable, IStandardStreamWriter
    {
        private readonly PooledBufferWriter<char> buffer;

        internal BufferedStreamWriter(int capacity, MemoryAllocator<char>? allocator)
            => buffer = new() { BufferAllocator = allocator, Capacity = capacity };

        internal IBufferWriter<char> BufferWriter => buffer;

        internal void CopyTo(IBufferWriter<char> output)
            => output.Write(buffer.WrittenMemory.Span);

        void IStandardStreamWriter.Write(string? value)
            => buffer.Write(value);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                buffer.Dispose();

            base.Dispose(disposing);
        }
    }

    private readonly BufferedStreamWriter output, error;

    internal MaintenanceConsole(IMaintenanceSession session, int capacity, MemoryAllocator<char>? allocator)
    {
        output = new(capacity, allocator);
        error = new(capacity, allocator);
        Session = session;
    }

    public IMaintenanceSession Session { get; }

    bool IStandardIn.IsInputRedirected => true;

    IStandardStreamWriter IStandardError.Error => error;

    public IBufferWriter<char> Error => error.BufferWriter;

    IStandardStreamWriter IStandardOut.Out => output;

    public IBufferWriter<char> Out => output.BufferWriter;

    internal bool SuppressErrorBuffer { get; set; }

    internal bool SuppressOutputBuffer { get; set; }

    internal bool PrintExitCode { get; set; }

    internal void Exit(int exitCode)
    {
        if (PrintExitCode)
            Session.Output.WriteString($"[{exitCode}]");

        (exitCode is 0 ? output : error).CopyTo(Session.Output);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            output.Dispose();
            error.Dispose();
        }

        base.Dispose(disposing);
    }
}