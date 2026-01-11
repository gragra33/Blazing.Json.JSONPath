using Blazing.Json.JSONPath.Tests.Fixtures;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.IntegrationTests;

/// <summary>
/// Integration tests validating RFC 9535 Table 2 examples.
/// These are end-to-end tests using the bookstore JSON from RFC Figure 1.
/// Source: https://www.rfc-editor.org/rfc/rfc9535.html#section-1.5-4
/// </summary>
public class RfcTable2ExampleTests
{
    [Fact]
    public void Example1_AllAuthors_ReturnsAllBookAuthors()
    {
        // $.store.book[*].author
        var authors = TestHelpers.QueryJsonAsStrings("$.store.book[*].author", RfcTestData.BookstoreJson);

        authors.Count.ShouldBe(4);
        authors.ShouldContain("Nigel Rees");
        authors.ShouldContain("Evelyn Waugh");
        authors.ShouldContain("Herman Melville");
        authors.ShouldContain("J. R. R. Tolkien");
    }

    [Fact]
    public void Example2_RecursiveAuthors_ReturnsAllAuthors()
    {
        // $..author
        var authors = TestHelpers.QueryJsonAsStrings("$..author", RfcTestData.BookstoreJson);

        authors.Count.ShouldBe(4);
        authors.ShouldContain("Nigel Rees");
        authors.ShouldContain("Evelyn Waugh");
        authors.ShouldContain("Herman Melville");
        authors.ShouldContain("J. R. R. Tolkien");
    }

    [Fact]
    public void Example3_AllStoreItems_ReturnsTwoItems()
    {
        // $.store.*
        var count = TestHelpers.QueryJsonCount("$.store.*", RfcTestData.BookstoreJson);

        count.ShouldBe(2); // book array + bicycle object
    }

    [Fact]
    public void Example4_AllPrices_ReturnsAllPriceValues()
    {
        // $.store..price
        var prices = TestHelpers.QueryJsonAsNumbers("$.store..price", RfcTestData.BookstoreJson);

        prices.Count.ShouldBe(5);
        prices.ShouldContain(8.95);
        prices.ShouldContain(12.99);
        prices.ShouldContain(8.99);
        prices.ShouldContain(22.99);
        prices.ShouldContain(399.0);
    }

    [Fact]
    public void Example5_ThirdBook_ReturnsMobyDick()
    {
        // $..book[2]
        var result = TestHelpers.QueryJson("$..book[2]", RfcTestData.BookstoreJson);

        result.Count.ShouldBe(1);
        var book = result.GetValues()[0];
        book.GetProperty("title").GetString().ShouldBe("Moby Dick");
        book.GetProperty("author").GetString().ShouldBe("Herman Melville");
    }

    [Fact]
    public void Example6_LastBook_ReturnsLordOfTheRings()
    {
        // $..book[-1]
        var result = TestHelpers.QueryJson("$..book[-1]", RfcTestData.BookstoreJson);

        result.Count.ShouldBe(1);
        var book = result.GetValues()[0];
        book.GetProperty("title").GetString().ShouldBe("The Lord of the Rings");
        book.GetProperty("author").GetString().ShouldBe("J. R. R. Tolkien");
    }

    [Fact]
    public void Example7_FirstTwoBooks_ReturnsTwoBooks()
    {
        // $..book[:2]
        var result = TestHelpers.QueryJson("$..book[:2]", RfcTestData.BookstoreJson);

        result.Count.ShouldBe(2);
        var titles = result.GetValues().Select(v => v.GetProperty("title").GetString()).ToList();
        titles.ShouldBe(new[] { "Sayings of the Century", "Sword of Honour" });
    }

    [Fact]
    public void Example8_BooksWithIsbn_ReturnsTwoBooks()
    {
        // $..book[?@.isbn]
        var result = TestHelpers.QueryJson("$..book[?@.isbn]", RfcTestData.BookstoreJson);

        result.Count.ShouldBe(2);
        var titles = result.GetValues().Select(v => v.GetProperty("title").GetString()).ToList();
        titles.ShouldContain("Moby Dick");
        titles.ShouldContain("The Lord of the Rings");
    }

    [Fact]
    public void Example9_CheapBooks_ReturnsBooksUnder10()
    {
        // $..book[?@.price < 10]
        var result = TestHelpers.QueryJson("$..book[?@.price < 10]", RfcTestData.BookstoreJson);

        result.Count.ShouldBe(2);
        var titles = result.GetValues().Select(v => v.GetProperty("title").GetString()).ToList();
        titles.ShouldContain("Sayings of the Century");
        titles.ShouldContain("Moby Dick");
    }

    [Fact]
    public void Example10_AllMembers_ReturnsAllNodesInDocument()
    {
        // $..*
        var count = TestHelpers.QueryJsonCount("$..*", RfcTestData.BookstoreJson);

        // Should return all nodes in the document (objects, arrays, values)
        count.ShouldBeGreaterThan(20);
    }
}
