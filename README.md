# ChromeCookiesFileUnlocker

## About

`ChromeCookiesFileUnlocker` is a C# library designed to unlock and copy the cookies file used by the Google Chrome browser. This is achieved by using the Restart Manager API to safely shut down any processes that are using the file, allowing it to be copied without causing any disruption.

This project is adapted and based on a [locked_cookie_test by Charles Machalow](https://gist.github.com/csm10495/e89e660ffee0030e8ef410b793ad6a7e)

## Features

- **Unlock Cookies File**: The library provides a method to unlock the cookies file used by Google Chrome. This is done by using the Restart Manager API to safely shut down any processes that are using the file.

- **Copy Cookies File**: Once the cookies file is unlocked, the library provides a method to copy the file to a specified location.

- **Progress Reporting**: The library provides an event that is triggered to report the progress of the file unlocking process.

## Usage

Here is a basic example of how to use the `ChromeCookiesFileUnlocker`:

```csharp
string cookiesPath = "<path to cookies file>"; // @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Network\Cookies";
string outputPath = "<path to output file>";

ChromeCookiesFileUnlocker.CopyFile(cookiesPath, outputPath);
```

In this example, `<path to cookies file>` should be replaced with the path to the cookies file you want to unlock and copy, and `<path to output file>` should be replaced with the path where you want the copied file to be saved.

## Dependencies

The `ChromeCookiesFileUnlocker` library uses the `System.Runtime.InteropServices` namespace for interop services, which allows managed code to interact with unmanaged code (like the Restart Manager API).

## Error Handling

The library includes a custom exception type, `FileUnlockerException`, which is thrown when an error occurs during the file unlocking or copying process. This exception includes a message that provides more information about the error.

## Contributing

Contributions to `ChromeCookiesFileUnlocker` are welcome! Please submit a pull request or create an issue on the project's GitHub page.

## REFERENCES

- https://learn.microsoft.com/en-us/archive/msdn-magazine/2007/april/net-matters-restart-manager-and-generic-method-compilation
- https://gist.github.com/csm10495/e89e660ffee0030e8ef410b793ad6a7e