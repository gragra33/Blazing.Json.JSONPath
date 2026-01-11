using BenchmarkDotNet.Running;
using Blazing.Json.JSONPath.Benchmarks;

// Entry point for RFC 9535 JSONPath performance benchmarks
// Usage examples:
//   dotnet run -c Release                                      # Interactive mode
//   dotnet run -c Release -- --anyCategories Parsing           # Run parsing benchmarks only
//   dotnet run -c Release -- --anyCategories Evaluation-Medium # Run medium dataset benchmarks
//   dotnet run -c Release -- --anyCategories Functions         # Run function benchmarks
//   dotnet run -c Release -- --list flat                       # List all benchmarks

var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);

if (args.Length == 0)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("│  RFC 9535 JSONPath - Performance Benchmarks             │");
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine();
    Console.WriteLine("Available benchmark categories:");
    Console.WriteLine("  1. Parsing          - Query parsing performance");
    Console.WriteLine("  2. Evaluation-Small - 10 items dataset");
    Console.WriteLine("  3. Evaluation-Medium - 100 items dataset");
    Console.WriteLine("  4. Evaluation-Large  - 1000 items dataset");
    Console.WriteLine("  5. End-to-End       - Parse + Evaluate combined");
    Console.WriteLine("  6. Functions        - Function expression performance");
    Console.WriteLine("  7. Memory           - Memory allocation tests");
    Console.WriteLine();
    Console.WriteLine("Quick selection:");
    Console.WriteLine("  [A] Run ALL benchmarks");
    Console.WriteLine("  [1-7] Run specific category");
    Console.WriteLine("  [Q] Quit");
    Console.WriteLine();
    Console.Write("Your choice: ");
    
    var choice = Console.ReadLine()?.Trim().ToUpper();
    
    switch (choice)
    {
        case "A":
            Console.WriteLine("\nRunning ALL JSONPath benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>();
            break;
        case "1":
            Console.WriteLine("\nRunning Parsing benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "Parsing"]);
            break;
        case "2":
            Console.WriteLine("\nRunning Evaluation-Small benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "Evaluation-Small"]);
            break;
        case "3":
            Console.WriteLine("\nRunning Evaluation-Medium benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "Evaluation-Medium"]);
            break;
        case "4":
            Console.WriteLine("\nRunning Evaluation-Large benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "Evaluation-Large"]);
            break;
        case "5":
            Console.WriteLine("\nRunning End-to-End benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "End-to-End"]);
            break;
        case "6":
            Console.WriteLine("\nRunning Function benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "Functions"]);
            break;
        case "7":
            Console.WriteLine("\nRunning Memory benchmarks...");
            BenchmarkRunner.Run<JsonPathBenchmarks>(null, args: ["--anyCategories", "Memory"]);
            break;
        case "Q":
            Console.WriteLine("Exiting...");
            return;
        default:
            Console.WriteLine();
            Console.WriteLine("Command-line usage:");
            Console.WriteLine("  dotnet run -c Release -- --anyCategories Parsing");
            Console.WriteLine("  dotnet run -c Release -- --list flat");
            Console.WriteLine();
            Console.WriteLine("See full options: dotnet run -c Release -- --help");
            break;
    }
}
else
{
    // Pass through to BenchmarkDotNet for advanced filtering
    switcher.Run(args);
}
