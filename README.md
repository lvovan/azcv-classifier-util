![Build status](https://github.com/lvovan/azcv-classifier-util/actions/workflows/dotnet.yml/badge.svg)
# azcv-classifier-util 

A command line utility to easily script [Azure Custom Vision](https://customvision.ai) image classifiers projects.

## Features

The tool is self-documented: run the executable without any arguments to get all the available verbs (actions). Run the executable with a verb as the first parameter to get all the available options for this verb.

- Tag directly from folders, using the folder names as tag values
- Crop images before uploading
- Augment your dataset by resizing and blurring your source images
- Script the creation, reset or deletion of projects
- Migrate a project from a source custom vision endpoint to another one
- Create a local backup of project

## Usage

For any command, it is possible to display usage examples using the `help` command. Example:

```console
foo@bar:~$ azcv-classifier-util help migrate
azcv-classifier-util 1.0.1
Copyright (C) 2023 Microsoft
USAGE:
Migrate a project to another custom vision resource (hosted in Japan). The new project will be named 'cvJapan':
  azcv-classifier-util migrate --destinationEndpoint https://japaneast.api.cognitive.microsoft.com/ --destinationKey destinationAPIKey --destinationName cvJapan --sourceEndpoint https://contoso.cognitiveservices.azure.com/ --sourceKey sourceAPIKey --sourceProjectId 8892946f-10c5-4884-94a4-da6c95f98d67
Create a copy of a project within the same custom vision resource. The new project will have an auto-generated name:
  azcv-classifier-util migrate --sourceEndpoint https://contoso.cognitiveservices.azure.com/ --sourceKey sourceAPIKey --sourceProjectId 8892946f-10c5-4884-94a4-da6c95f98d67

  --sourceEndpoint         Required. Custom Vision endpoint hosting the source project to be migrated.

  --destinationEndpoint    Target Custom Vision endpoint. If not specified, the sourceEndpoint will be used.

  --sourceKey              Required.  Source Custom Vision endpoint key.

  --destinationKey         Target Custom Vision endpoint key. If not specified, the sourceKey will be used

  --sourceProjectId        Required. Project ID to be migrated

  --destinationName        Optional, name of the project to use instead of auto-generated name.

  --help                   Display this help screen.

  --version                Display version information.
```

## Build & Debug

This utility is built using .NET Core and relies on the Azure Custom Vision SDK. Compiling the utility requires the [.NET Core 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) (not just the runtime).

You can build the application by simply running:

```sh
dotnet build
```

This should create an executable in the **bin/Debug/netcore7.0** directory, from which you can then execute the `azcv-classifier-util` (`azcv-classifier-util.exe` on Windows) executable.
