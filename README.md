# Code Bundler CLI Application

This project is a C# command-line application that allows users to bundle code files from different programming languages into a single file. It provides flexibility for users to choose which files to include, sort them, remove empty lines, and more. Additionally, it provides a feature to generate a response file (`.rsp`) to simplify the command execution process.

## Overview

The application includes two main commands:

1. **`bundle`**: Bundles code files into a single output file based on the user's preferences.
2. **`create-rsp`**: Simplifies the use of the `bundle` command by creating a response file that contains the full command with specified options.

## Features

### `bundle` Command:

Bundles code files from selected programming languages into a single file.

#### Options:
- **`--language` (`-l`)**: Specify which programming languages to include. If `all` is selected, all files in the current directory will be included.
- **`--output` (`-o`)**: Specify the output file name and path. The file will be saved in the current directory by default, or the user can provide a full path.
- **`--note` (`-n`)**: Include the source code file paths as comments in the output bundle.
- **`--sort` (`-s`)**: Specify the sorting method for the files (`name` or `type`).
- **`--remove-empty-lines` (`-r`)**: Remove empty lines from the code files before bundling them.
- **`--author` (`-a`)**: Specify the author of the bundle, which will be added as a comment at the top of the bundle.

### `create-rsp` Command:

Prompts the user for all the necessary options and generates a `.rsp` file that contains the full command. The `.rsp` file can later be executed with `dotnet @fileName.rsp`.

