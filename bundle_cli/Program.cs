﻿using System;
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


var sortOption = new Option<string>(
    "--sort",
    "Sort files by 'name' (default) or 'type' (by file extension)"
);

bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(includeSourceOption);
bundleCommand.AddOption(sortOption);


bundleCommand.SetHandler((string language, FileInfo output, bool includeSource, string sort) =>
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
                outputFile.WriteLine("----------------------------------------------");
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

}, languageOption, outputOption, includeSourceOption, sortOption);

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

rootCommand.InvokeAsync(args);