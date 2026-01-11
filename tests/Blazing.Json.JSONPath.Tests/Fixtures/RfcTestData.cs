namespace Blazing.Json.JSONPath.Tests.Fixtures;

/// <summary>
/// RFC 9535 test data fixtures.
/// Contains standard JSON examples from the RFC specification.
/// </summary>
public static class RfcTestData
{
    /// <summary>
    /// RFC 9535 Figure 1 - Bookstore JSON example (used throughout the specification).
    /// Source: https://www.rfc-editor.org/rfc/rfc9535.html#section-1.5
    /// </summary>
    public static readonly string BookstoreJson = """
    {
      "store": {
        "book": [
          {
            "category": "reference",
            "author": "Nigel Rees",
            "title": "Sayings of the Century",
            "price": 8.95
          },
          {
            "category": "fiction",
            "author": "Evelyn Waugh",
            "title": "Sword of Honour",
            "price": 12.99
          },
          {
            "category": "fiction",
            "author": "Herman Melville",
            "title": "Moby Dick",
            "isbn": "0-553-21311-3",
            "price": 8.99
          },
          {
            "category": "fiction",
            "author": "J. R. R. Tolkien",
            "title": "The Lord of the Rings",
            "isbn": "0-395-19395-8",
            "price": 22.99
          }
        ],
        "bicycle": {
          "color": "red",
          "price": 399
        }
      }
    }
    """;

    /// <summary>
    /// Simple test data for basic queries.
    /// </summary>
    public static readonly string SimpleObjectJson = """
    {
      "name": "John Doe",
      "age": 30,
      "email": "john@example.com"
    }
    """;

    /// <summary>
    /// Simple array test data.
    /// </summary>
    public static readonly string SimpleArrayJson = """
    ["apple", "banana", "cherry", "date"]
    """;

    /// <summary>
    /// Nested object test data.
    /// </summary>
    public static readonly string NestedObjectJson = """
    {
      "user": {
        "profile": {
          "name": "Jane Smith",
          "location": {
            "city": "New York",
            "country": "USA"
          }
        }
      }
    }
    """;

    /// <summary>
    /// Array with objects test data.
    /// </summary>
    public static readonly string ArrayWithObjectsJson = """
    [
      {"id": 1, "name": "Alice", "score": 85},
      {"id": 2, "name": "Bob", "score": 92},
      {"id": 3, "name": "Charlie", "score": 78},
      {"id": 4, "name": "Diana", "score": 95}
    ]
    """;
}
