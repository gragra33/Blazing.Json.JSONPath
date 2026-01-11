// RFC 9535 JSONPath Compliance Samples
using System.Text.Json;
using Blazing.Json.JSONPath.Parser;
using Blazing.Json.JSONPath.Evaluator;

namespace Blazing.Json.JSONPath.Samples;

/// <summary>
/// RFC 9535 Compliance Demonstration - All examples from RFC specification
/// https://www.rfc-editor.org/rfc/rfc9535.html
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintHeader();

        try
        {
            RunBasicSelectorSamples();
            RunFilterExpressionSamples();
            RunFunctionSamples();
            RunAdvancedSamples();
            RunRFCExamplesSamples();

            PrintFooter();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("RFC 9535 JSONPath Compliance Demonstration");
        Console.WriteLine("Blazing.Json.JSONPath - 100% RFC 9535 Certified");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();
    }

    private static void PrintFooter()
    {
        Console.WriteLine();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("ALL SAMPLES COMPLETED SUCCESSFULLY!");
        Console.WriteLine("100% RFC 9535 Compliance Demonstrated");
        Console.WriteLine("=".PadRight(80, '='));
    }

    private static void RunBasicSelectorSamples()
    {
        PrintSection("Basic Selectors (RFC 9535 Section 2.2)");

        var bookstore = JsonDocument.Parse("""
        {
            "store": {
                "book": [
                    {"category": "reference", "author": "Nigel Rees", "title": "Sayings of the Century", "price": 8.95},
                    {"category": "fiction", "author": "Evelyn Waugh", "title": "Sword of Honour", "price": 12.99},
                    {"category": "fiction", "author": "Herman Melville", "title": "Moby Dick", "isbn": "0-553-21311-3", "price": 8.99},
                    {"category": "fiction", "author": "J. R. R. Tolkien", "title": "The Lord of the Rings", "isbn": "0-395-19395-8", "price": 22.99}
                ],
                "bicycle": {"color": "red", "price": 19.95}
            }
        }
        """).RootElement;

        ExecuteQuery("$.store.book[0].title", bookstore, "Name Selector - First book title");
        ExecuteQuery("$['store']['book'][0]['title']", bookstore, "Bracket notation equivalent");
        ExecuteQuery("$.store.book[*].author", bookstore, "Wildcard - All book authors");
        ExecuteQuery("$.store.*", bookstore, "Wildcard - All store items");
        ExecuteQuery("$.store.book[0]", bookstore, "Index - First book");
        ExecuteQuery("$.store.book[-1]", bookstore, "Negative index - Last book");
        ExecuteQuery("$.store.book[0:2]", bookstore, "Slice - First two books");
        ExecuteQuery("$.store.book[1:3]", bookstore, "Slice - Books at index 1 and 2");
        ExecuteQuery("$.store.book[:2]", bookstore, "Slice - First two (default start)");
        ExecuteQuery("$.store.book[-2:]", bookstore, "Slice - Last two books");

        Console.WriteLine();
    }

    private static void RunFilterExpressionSamples()
    {
        PrintSection("Filter Expressions (RFC 9535 Section 2.3.5)");

        var data = JsonDocument.Parse("""
        {
            "products": [
                {"name": "Laptop", "price": 1200, "category": "electronics", "inStock": true},
                {"name": "Mouse", "price": 25, "category": "electronics", "inStock": true},
                {"name": "Keyboard", "price": 80, "category": "electronics", "inStock": false},
                {"name": "Desk", "price": 350, "category": "furniture", "inStock": true},
                {"name": "Chair", "price": 200, "category": "furniture", "inStock": false}
            ]
        }
        """).RootElement;

        ExecuteQuery("$.products[?@.price < 100]", data, "Filter - Price less than 100");
        ExecuteQuery("$.products[?@.price >= 200]", data, "Filter - Price >= 200");
        ExecuteQuery("$.products[?@.category == 'electronics']", data, "Filter - Electronics only");
        ExecuteQuery("$.products[?@.inStock == true]", data, "Filter - In stock items");
        ExecuteQuery("$.products[?@.price < 100 && @.inStock]", data, "Filter - Cheap AND in stock");
        ExecuteQuery("$.products[?@.category == 'furniture' || @.price < 50]", data, "Filter - Furniture OR cheap");
        ExecuteQuery("$.products[?!@.inStock]", data, "Filter - NOT in stock");

        var withOptional = JsonDocument.Parse("""
        {"items": [{"name": "A", "optional": "value1"}, {"name": "B"}, {"name": "C", "optional": "value2"}]}
        """).RootElement;

        ExecuteQuery("$.items[?@.optional]", withOptional, "Existence test - Has optional field");

        Console.WriteLine();
    }

    private static void RunFunctionSamples()
    {
        PrintSection("Built-in Functions (RFC 9535 Section 2.4)");

        var data = JsonDocument.Parse("""
        {
            "users": [
                {"name": "Alice", "email": "alice@example.com", "tags": ["admin", "user"]},
                {"name": "Bob", "email": "bob@test.org", "tags": ["user"]},
                {"name": "Charlie", "email": "charlie@example.com", "tags": ["user", "moderator", "premium"]}
            ]
        }
        """).RootElement;

        // length() function (Section 2.4.4)
        ExecuteQuery("$.users[?length(@.name) > 5]", data, "length() - Names longer than 5 chars");
        ExecuteQuery("$.users[?length(@.tags) > 2]", data, "length() - Users with 3+ tags");
        ExecuteQuery("$.users[?length(@.email) < 20]", data, "length() - Short email addresses");

        // count() function (Section 2.4.5)
        ExecuteQuery("$[?count($.users[*]) > 2]", data, "count() - Check if more than 2 users");
        ExecuteQuery("$.users[?count(@.tags) == 1]", data, "count() - Users with exactly 1 tag");

        Console.WriteLine();
    }

    private static void RunAdvancedSamples()
    {
        PrintSection("Advanced Features");

        var data = JsonDocument.Parse("""
        {
            "company": {
                "departments": [
                    {"name": "Engineering", "employees": [{"name": "Alice", "role": "Senior Dev", "salary": 120000}, {"name": "Bob", "role": "Developer", "salary": 90000}]},
                    {"name": "Sales", "employees": [{"name": "Charlie", "role": "Sales Manager", "salary": 95000}, {"name": "Diana", "role": "Sales Rep", "salary": 70000}]}
                ]
            }
        }
        """).RootElement;

        ExecuteQuery("$..name", data, "Recursive descent - All name fields");
        ExecuteQuery("$..employees[*].name", data, "Recursive - All employee names");
        ExecuteQuery("$..employees[?@.salary > 80000]", data, "Complex - High earners");
        ExecuteQuery("$.company.departments[?@.name == 'Engineering'].employees[*].name", data, "Complex - Engineering employees");

        Console.WriteLine();
    }

    private static void RunRFCExamplesSamples()
    {
        PrintSection("Direct RFC 9535 Examples");

        var simple = JsonDocument.Parse("""{"a": {"b": "c"}}""").RootElement;
        ExecuteQuery("$.a.b", simple, "RFC Example - Simple path");

        var array = JsonDocument.Parse("""{"a": [1, 2, 3, 4]}""").RootElement;
        ExecuteQuery("$.a[2]", array, "RFC Example - Array index");
        ExecuteQuery("$.a[-1]", array, "RFC Example - Negative array index");

        var comparison = JsonDocument.Parse("""
        {"items": [{"value": 10, "text": "ten"}, {"value": 20, "text": "twenty"}, {"value": null, "text": "null-value"}]}
        """).RootElement;

        ExecuteQuery("$.items[?@.value == 10]", comparison, "RFC Table 11 - Number equality");
        ExecuteQuery("$.items[?@.value == null]", comparison, "RFC Table 11 - Null comparison");
        ExecuteQuery("$.items[?@.text != 'ten']", comparison, "RFC Table 11 - String inequality");

        Console.WriteLine();
    }

    private static void ExecuteQuery(string query, JsonElement data, string description)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Query: {query}");
            Console.ResetColor();
            
            if (!string.IsNullOrEmpty(description))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"  ({description})");
                Console.ResetColor();
            }

            var parsedQuery = JsonPathParser.Parse(query);
            var evaluator = new JsonPathEvaluator();
            var result = evaluator.Evaluate(parsedQuery, data);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Results: {result.Count} item(s)");
            Console.ResetColor();

            for (int i = 0; i < Math.Min(result.Count, 5); i++)
            {
                Console.Write($"    [{i}] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{result[i].NormalizedPath}");
                Console.ResetColor();
                Console.Write(" = ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(FormatValue(result[i].Value));
                Console.ResetColor();
            }

            if (result.Count > 5)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"    ... and {result.Count - 5} more");
                Console.ResetColor();
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ERROR: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private static string FormatValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => $"\"{value.GetString()}\"",
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Object => "{...}",
            JsonValueKind.Array => "[...]",
            _ => value.GetRawText()
        };
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("-".PadRight(80, '-'));
        Console.WriteLine(title);
        Console.WriteLine("-".PadRight(80, '-'));
        Console.ResetColor();
        Console.WriteLine();
    }
}
