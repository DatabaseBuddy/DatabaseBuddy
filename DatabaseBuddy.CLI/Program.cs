using DatabaseBuddy.CLI.DynamicCLIMenu;

var List = new List<string>();
for (int i = 0; i < 10; i++)
    List.Add($"Item{i}");

var Result = new DynamicCLIMenu("Test title", List).Run();

foreach (var Item in Result)
    Console.WriteLine($"{Item.Key}: {Item.Value}");

Console.ReadKey();