using NDesk.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

var folder = AppDomain.CurrentDomain.BaseDirectory;
var filter = "*";
var dump = false;
var help = false;

var p = new OptionSet() {
    { "d|directory=", "{FOLDER} do watch.", v => folder = v },
    { "f|filter=", "Example: *.txt", v => filter = v },
    { "dump", "Print file content as a text.", v => dump = true },
    { "?|h|help", "Show options.", v => help = true }
};

var extra = p.Parse(args);
if(args.Length == 0 || extra.Count > 0 || help)
{
    Console.WriteLine("Parameters:");
    p.WriteOptionDescriptions(Console.Out);
    return;
}

var watcher = new FileSystemWatcher(folder, filter);

watcher.NotifyFilter = NotifyFilters.Attributes
                     | NotifyFilters.CreationTime
                     | NotifyFilters.DirectoryName
                     | NotifyFilters.FileName
                     | NotifyFilters.LastAccess
                     | NotifyFilters.LastWrite
                     | NotifyFilters.Security
                     | NotifyFilters.Size;

watcher.Changed += Catch;
watcher.Created += Catch;
watcher.Deleted += Catch;
watcher.Renamed += Catch;
watcher.Error += OnError;

watcher.IncludeSubdirectories = true;
watcher.EnableRaisingEvents = true;

Thread.Sleep(-1);

void Catch(object sender, FileSystemEventArgs e)
{
    var jsonOptions = new JsonSerializerOptions();
    jsonOptions.Converters.Add(new JsonStringEnumConverter());
    Console.WriteLine(JsonSerializer.Serialize(e, e.GetType(), jsonOptions));
    if(dump && e.ChangeType != WatcherChangeTypes.Deleted)
    {
        try
        {
            Console.WriteLine("--- Dump Start ---\n" + File.ReadAllText(e.FullPath) + "\n--- Dump End ---");
        }
        catch(IOException ex)
        {
            Console.WriteLine("--- Dump Error ---\n" + ex.Message + "\n--- Dump End ---");
        }
    }
}

static void OnError(object sender, ErrorEventArgs e) =>  PrintException(e.GetException());

static void PrintException(Exception? ex)
{
    if (ex != null)
    {
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine("Stacktrace:");
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine();
        PrintException(ex.InnerException);
    }
}