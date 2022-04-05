# FileUploadToDisk

An exercise in making a web app in ASP.NET Core for uploading/downloading large files by streaming directly to disk, without storing in memory, on the server.

### Features
* Capable of handling multiple uploaded files at once from client.
* Saves file metadata to an Sqlite memory database and files to a folder.
	* For a real use case the memory server should be changed to a persistent database.
* At upload files are initially named with a random filename. This new filename and original filename is stored together in DB.
	* In a real use case file should be moved to permanent folder, and renamed with a hash of the file contents or the like.
* At download DownloadFile action sends original filename and MimeType to client.

## Getting Started

To compile or continue development try this:

Install **git** and **Visual Studio 2022**, then open a git console:

```
cd .\suitable\project\folder
git clone <address_to_this_repo>
```

Open ...\suitable\project\folder\FuleUploadToDisk\FileUploadToDisk.sln in Visual Studio.
To build and run click the Run-button for the project.

## Acknowledgements

Other sources of inspiration and knowledge:
* [Upload files in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-6.0)
