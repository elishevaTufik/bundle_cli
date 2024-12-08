using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

var bundleCommand = new Command(
    "bundle", "bundle code file to a single code file"
);

var languageOption = new Option<string>(
    "--language",
    "Specify the programming languages to include in the bundle. Use 'all' for all files."
)
{
    IsRequired = true
};

bundleCommand.AddOption(languageOption);

bundleCommand.SetHandler((string language) =>
{
    try
    {
        string[] validExtensions = language.ToLower() == "all"
            ? new string[] { ".cs", ".js", ".py", ".java", ".html", ".css" }
            : GetExtensionsForLanguage(language.ToLower());

        // נתיב לתיקייה הנוכחית
        string directoryPath = Directory.GetCurrentDirectory();

        // קבלת כל הקבצים בתיקייה הנוכחית, שמתאימים להרחבות שבחרנו
        var filesToBundle = Directory.GetFiles(directoryPath)
            .Where(file => validExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // אם לא נמצאו קבצים מתאימים, להוציא הודעת שגיאה
        if (!filesToBundle.Any())
        {
            Console.WriteLine("No files found matching the specified languages.");
            //return;
        }

        // יצירת שם קובץ חדש עבור ה-bundle
        string outputFileName = "bundle_output.txt";
        using (var outputFile = new StreamWriter(outputFileName))
        {
            foreach (var file in filesToBundle)
            {
                // קריאת תוכן כל קובץ והדפסתו לקובץ הפלט
                var content = File.ReadAllText(file);
                outputFile.WriteLine($"// File: {Path.GetFileName(file)}");
                outputFile.WriteLine(content);
                outputFile.WriteLine(); // השארת רווח בין הקבצים
            }
        }

        Console.WriteLine($"Bundle created successfully. Output file: {outputFileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");
    }
}, languageOption);

// פונקציה שמחזירה את הסיומות המתאימות לשפות תכנות
string[] GetExtensionsForLanguage(string language)
{
    return language switch
    {
        "csharp" => new string[] { ".cs" },
        "javascript" => new string[] { ".js" },
        "python" => new string[] { ".py" },
        "java" => new string[] { ".java" },
        "html" => new string[] { ".html" },
        "css" => new string[] { ".css" },
        _ => throw new ArgumentException("Unsupported language")
    };
}

var rootCommand = new RootCommand("Root Command for file bundler ");

rootCommand.AddCommand(bundleCommand);

rootCommand.InvokeAsync(args);