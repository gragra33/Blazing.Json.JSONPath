using System.Buffers;
using System.Collections.Immutable;

namespace Blazing.Json.JSONPath.Evaluator;

/// <summary>
/// Provides efficient construction of <see cref="Nodelist"/> instances.
/// Uses ArrayPool for zero-allocation temporary buffers.
/// </summary>
/// <remarks>
/// This builder is mutable and not thread-safe.
/// Call <see cref="ToNodelist"/> to create the final immutable nodelist.
/// </remarks>
public sealed class NodelistBuilder : IDisposable
{
    private JsonNode[] _buffer;
    private int _count;
    private bool _disposed;

    private const int InitialCapacity = 16;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodelistBuilder"/> class.
    /// </summary>
    /// <param name="capacity">The initial capacity (rented from ArrayPool).</param>
    public NodelistBuilder(int capacity = InitialCapacity)
    {
        _buffer = ArrayPool<JsonNode>.Shared.Rent(capacity);
        _count = 0;
        _disposed = false;
    }

    /// <summary>
    /// Gets the current count of nodes in the builder.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Adds a node to the builder.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void Add(JsonNode node)
    {
        EnsureCapacity(_count + 1);
        _buffer[_count++] = node;
    }

    /// <summary>
    /// Adds multiple nodes to the builder.
    /// </summary>
    /// <param name="nodes">The nodes to add.</param>
    public void AddRange(IEnumerable<JsonNode> nodes)
    {
        foreach (var node in nodes)
        {
            Add(node);
        }
    }

    /// <summary>
    /// Adds nodes from another nodelist to the builder.
    /// </summary>
    /// <param name="nodelist">The nodelist to add.</param>
    public void AddRange(Nodelist nodelist)
    {
        if (nodelist.Count == 0)
            return;

        EnsureCapacity(_count + nodelist.Count);

        foreach (var node in nodelist)
        {
            _buffer[_count++] = node;
        }
    }

    /// <summary>
    /// Clears all nodes from the builder.
    /// </summary>
    public void Clear()
    {
        // Clear references for GC
        if (_count > 0)
        {
            Array.Clear(_buffer, 0, _count);
        }
        _count = 0;
    }

    /// <summary>
    /// Builds an immutable <see cref="Nodelist"/> from the current nodes.
    /// </summary>
    /// <param name="deduplicate">Whether to deduplicate by normalized path.</param>
    /// <returns>An immutable <see cref="Nodelist"/>.</returns>
    public Nodelist ToNodelist(bool deduplicate = false)
    {
        if (_count == 0)
            return Nodelist.Empty;

        var nodelist = Nodelist.Create(_buffer.AsSpan(0, _count));

        return deduplicate ? nodelist.Deduplicate() : nodelist;
    }

    /// <summary>
    /// Ensures the buffer has sufficient capacity.
    /// </summary>
    /// <param name="requiredCapacity">The required capacity.</param>
    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _buffer.Length)
            return;

        // Grow by 2x or to required capacity, whichever is larger
        var newCapacity = Math.Max(_buffer.Length * 2, requiredCapacity);
        var newBuffer = ArrayPool<JsonNode>.Shared.Rent(newCapacity);

        // Copy existing data
        Array.Copy(_buffer, newBuffer, _count);

        // Return old buffer to pool
        ArrayPool<JsonNode>.Shared.Return(_buffer, clearArray: true);

        _buffer = newBuffer;
    }

    /// <summary>
    /// Releases resources used by the builder.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_buffer != null)
        {
            ArrayPool<JsonNode>.Shared.Return(_buffer, clearArray: true);
            _buffer = null!;
        }

        _count = 0;
        _disposed = true;
    }
}
