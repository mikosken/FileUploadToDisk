// This utility class is modified from code in a Microsoft documentation sample app at:
// https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/mvc/models/file-uploads/samples/3.x/SampleApp/Controllers/StreamingController.cs
//
using FileUploadToDisk.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SampleApp.Utilities;
using System.Text;

namespace FileUploadToDisk.Utilities
{
	public static class FileStreamingHelper
	{
		private static readonly FormOptions _defaultFormOptions = new FormOptions();
		// Generates a temporary filename for each file encountered in multipart request and saves to specified folder.
		public static async Task<List<FileMetadata>> StreamFilesToDisk(HttpRequest request, string targetFilePath)
		{
			// Keep track of metadata.
			// Key is randomly generated safe filename, value is original untrusted filename.
			List<FileMetadata> writtenFiles = new List<FileMetadata>();

			if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
			{
				throw new Exception($"Expected a multipart request, but got {request.ContentType}");
			}
			// Used to accumulate all the form url encoded key value pairs in the 
			// request.
			var formAccumulator = new KeyValueAccumulator();

			var boundary = MultipartRequestHelper.GetBoundary(
				MediaTypeHeaderValue.Parse(request.ContentType),
				_defaultFormOptions.MultipartBoundaryLengthLimit);
			var reader = new MultipartReader(boundary, request.Body);
			var section = await reader.ReadNextSectionAsync();

			while (section != null)
			{
				ContentDispositionHeaderValue contentDisposition;
				var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);
				if (hasContentDispositionHeader)
				{
					if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
					{
						var trustedFileNameForFileStorage = Path.GetRandomFileName();
						// Metadata for uploaded file.
						var meta = new FileMetadata();
						meta.Filename = trustedFileNameForFileStorage;
						meta.OriginalFilename = contentDisposition.FileName.Value;
						meta.MimeType = section.ContentType;

						using (var targetStream = System.IO.File.Create(
							Path.Combine(targetFilePath, trustedFileNameForFileStorage)))
						{
							await section.Body.CopyToAsync(targetStream);
							writtenFiles.Add(meta);
							// Log that file has been written here.
						}
					}
					else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
					{
						// Content-Disposition: form-data; name="key"
						//
						// value
						// Do not limit the key name length here because the 
						// multipart headers length limit is already in effect.
						var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
						var encoding = GetEncoding(section);
						using (var streamReader = new StreamReader(
							section.Body,
							encoding,
							detectEncodingFromByteOrderMarks: true,
							bufferSize: 1024,
							leaveOpen: true))
						{
							// The value length limit is enforced by MultipartBodyLengthLimit
							var value = await streamReader.ReadToEndAsync();
							if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
							{
								value = String.Empty;
							}
							formAccumulator.Append(key.Value, value); // For .NET Core <2.0 remove ".Value" from key
							if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
							{
								throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
							}
						}
					}
				}
				// Drains any remaining section body that has not been consumed and
				// reads the headers for the next section.
				section = await reader.ReadNextSectionAsync();
			}

			return writtenFiles;
		}

		private static Encoding GetEncoding(MultipartSection section)
		{
			MediaTypeHeaderValue mediaType;
			var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
			// UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
			// most cases.
			if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
			{
				return Encoding.UTF8;
			}
			return mediaType.Encoding;
		}
	}
}
