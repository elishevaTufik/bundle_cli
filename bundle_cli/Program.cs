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
  new string[] { "--output", "-o" },
  "Specify the output file path"
);

var languageOption = new Option<string>(
    new string[] { "--language", "-l" },
    "Specify the programming languages to include in the bundle. Use 'all' for all files."
)
{
    IsRequired = true
};

var includeSourceOption = new Option<bool>(
    new string[] { "--include-source","-i" },
    "Include the source code file path as a comment in the bundle"
);

var sortOption = new Option<string>(
    new string[] { "--sort", "-s" },
    () => "name",
    "Sort files by 'name' (default) or 'type' (by file extension)"
);

var removeOption = new Option<bool>(
    new string[] { "--remove", "-r" },
    "Remove empty lines from the source code before bundling"
);

var authorOption = new Option<string>(
    new string[] { "--author","a" },
    "Specify the author of the bundle (this will be added as a comment at the top of the bundle)"
);


bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(includeSourceOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeOption);
bundleCommand.AddOption(authorOption);

// Create the create-rsp command to generate a response file
var createRspCommand = new Command(
    "create-rsp",
    "Generate a response file for bundling code files"
);

createRspCommand.SetHandler(async () =>
{
    try
    {
        // Ask for user input for each option
        Console.WriteLine("Enter the desired parameters for the bundle command:");

        Console.Write("Language (csharp, javascript, python, java, html, css, c, c++, c#, all): ");
        string language = Console.ReadLine();

        Console.Write("Output file path or Output file name");
        string outputPath = Console.ReadLine();

        Console.Write("Include source (true/false): ");
        bool includeSource = bool.Parse(Console.ReadLine());

        Console.Write("Sort files (name/type): ");
        string sort = Console.ReadLine();

        Console.Write("Remove empty lines (true/false): ");
        bool remove = bool.Parse(Console.ReadLine());

        Console.Write("Author (optional): ");
        string author = Console.ReadLine();

        // Generate the full command line based on user input
        string commandLine = $"dotnet run --cmd --language {language} --output {outputPath} --include-source {includeSource} --sort {sort} --remove {remove}";

        if (!string.IsNullOrEmpty(author))
        {
            commandLine += $" --author \"{author}\"";
        }

        // Create and write the response file
        string responseFileName = "response.rsp";
        File.WriteAllText(responseFileName, commandLine);

        Console.WriteLine($"Response file created successfully: {responseFileName}");
    }

    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
});


bundleCommand.SetHandler((string language, FileInfo output, bool includeSource, string sort, bool remove, string author) =>
{
    try
    {
        string[] validExtensions = language.ToLower() == "all"
            ? new string[] { ".cs", ".js", ".py", ".java", ".html", ".css", ".ipynb" }
            : GetExtensionsForLanguage(language.ToLower());

        // Get the current directory path
        string directoryPath = Directory.GetCurrentDirectory();

        // Get all the files in the current directory matching the selected extensions
        var filesToBundle = Directory.GetFiles(directoryPath)
            .Where(file => validExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // If no files are found, print an error message
        if (!filesToBundle.Any())
        {
            Console.WriteLine("No files found matching the specified languages.");
            return;
        }

        // Sort files based on the user's choice
        filesToBundle = sort.ToLower() switch
        {
            "type" => filesToBundle.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToList(),
            _ => filesToBundle.OrderBy(f => Path.GetFileName(f)).ToList()  // Default sort by file name
        };

        // Create the output file
        string outputFileName = output.FullName;
        using (var outputFile = new StreamWriter(outputFileName))
        {
            // If author is provided, write the author's name as a comment at the top

            if (!string.IsNullOrEmpty(author))
            {
                outputFile.WriteLine($"// Author: {author}");
                outputFile.WriteLine($"// Date: {DateTime.Now:yyyy-MM-dd}");
                outputFile.WriteLine();  
            }

            foreach (var file in filesToBundle)
            {
                // If the flag is true, include the source file path as a comment
                if (includeSource)
                {
                    outputFile.WriteLine($"// Source file: {Path.GetRelativePath(directoryPath, file)}");
                }

                // Read the content of each file and write it to the output file
                var content = File.ReadAllText(file);

                // If remove flag is true, remove empty lines from the content
                if (remove)
                {
                    content = string.Join(Environment.NewLine, content
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .Where(line => !string.IsNullOrWhiteSpace(line)));
                }

                // Write the content to the output file
                outputFile.WriteLine();
                outputFile.WriteLine(content);
                outputFile.WriteLine("----------------------------------------------");
                outputFile.WriteLine();
            }
        }

        Console.WriteLine($"Bundle created successfully. Output file: {outputFileName}");
    }
    
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"ERROR: Access denied to path: {ex.Message}");
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

}, languageOption, outputOption, includeSourceOption, sortOption, removeOption, authorOption);

// Function to return valid extensions for different languages
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

var rootCommand = new RootCommand("Root Command for file bundler");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

rootCommand.InvokeAsync(args);