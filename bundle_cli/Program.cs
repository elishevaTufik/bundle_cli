using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

var bundleCommand = new Command(
    "cmd", 
    "bundle code files to a single code file"
);

var outputOption = new Option<FileInfo>(
    "--output",
    "Specify the output file path"
);

var languageOption = new Option<string>(
    "--language",
    "Specify the programming languages to include in the bundle. Use 'all' for all files."
)
{
    IsRequired = true,
};


var includeSourceOption = new Option<bool>(
    "--include-source",
    "Include the source code file path as a comment in the bundle"
);

bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(includeSourceOption);

bundleCommand.SetHandler((string language, FileInfo output, bool includeSource) =>
{
    try
    {
        string[] validExtensions = language.ToLower() == "all"
            ? new string[] { ".cs", ".js", ".py", ".java", ".html", ".css", ".ipynb" }
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
            return;
        }

        // יצירת שם קובץ חדש עבור ה-bundle
        string outputFileName = output.FullName;
        using (var outputFile = new StreamWriter(outputFileName))
        {
            foreach (var file in filesToBundle)
            {
                // If the flag is true, include the source file path as a comment
                if (includeSource)
                {
                    outputFile.WriteLine($"// Source file: {Path.GetRelativePath(directoryPath, file)}");
                }

                // Read the content of each file and write it to the output file
                var content = File.ReadAllText(file);
                //outputFile.WriteLine($"// File: {Path.GetFileName(file)}");
                outputFile.WriteLine();
                outputFile.WriteLine(content);
                outputFile.WriteLine();
            }
        }

        Console.WriteLine($"Bundle created successfully. Output file: {outputFileName}");
    }

    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine($"ERROR : invalid path {ex.Message}");
    }

    catch (NullReferenceException ex)
    {
        Console.WriteLine($"ERROR : {ex.Message}");
    }

    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }

}, languageOption, outputOption, includeSourceOption);

// פונקציה שמחזירה את הסיומות המתאימות לשפות תכנות
string[] GetExtensionsForLanguage(string language)
{
    return language switch
    {
        "csharp" => new string[] { ".cs" },
        "javascript" => new string[] { ".js" },
        "python" => new string[] { ".ipynb" },
        "java" => new string[] { ".java" },
        "html" => new string[] { ".html" },
        "css" => new string[] { ".css" },
        "c" => new string[] { ".c" },
        "c++" => new string[] { ".cpp" },
        "c#" => new string[] { ".cs" },

        _ => throw new ArgumentException("Unsupported language")
    };
}

var rootCommand = new RootCommand("Root Command for file bundler ");

rootCommand.AddCommand(bundleCommand);

rootCommand.InvokeAsync(args);