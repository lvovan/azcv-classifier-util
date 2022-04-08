# azcv-classifier-util
A command line utility to easily script [Azure Custom Vision](https://customvision.ai) image classifiers projects.

### Features
- Tag directly from folders, using the folder names as tag values
- Crop images before uploading
- Augment your dataset by resizing and blurring your source images
- Script the creation, reset or deletion of projects


## Build
This utility is built using .NET Core and relies on the Azure Custom Vision SDK. Compiling the utility requires the [.NET Core 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (not just the runtime).

You can build the application by simply running:

`dotnet build`

This should create an executable in the **bin/Debug/netcore6.0** directory, from which you can then execute the `azcv-classifier-util` (`azcv-classifier-util.exe` on Windows) executable.

## Usage
The tool is self-documented: run the executable without any arguments to get all the available verbs (actions). Run the executable with a verb as the first parameter to get all the available options for this verb.

Examples:

- Upload the **~/mytaggedimages** folder and its subfolders to Custom Vision project with id 12345: 
    ```
    azcv-classifier-util tag -p 123456 -f ~/mytaggedimages
    ```

- Start training project with id 12345:

    ```
    azcv-classifier-util train -p 123456
    ```

- Upload a folder and its subfolders to Custom Vision project with id 12345, using only the rectangle area at (100,100) width=200 height=300 for training :

    ```
    azcv-classifier-util tag -p 123456 -f ~/mytaggedimages -c 100,100,200,300
    ```

- Create 10 augmented versions of **my_image.jpeg** by generating combinations of: random rotations between -5 and +5 degrees, random resizes between -7 and +7 percent of the original size, and random blurring

    ```
    azcv-classifier-util augment -c 10 -r 5 -s 7 -b my_image.jpeg
    ```
