using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace File.Helpers
{
    public class ZipDownloadHelper
    {
        public static Task<byte[]> GetFileData(string file, CancellationToken cancellationToken = default)
            => System.IO.File.ReadAllBytesAsync(file, cancellationToken);

        public static async IAsyncEnumerable<(string path,byte[] data)> GetFilesDataList(List<string> files, 
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var file in files)
            {
                yield return new (file,await GetFileData(file, cancellationToken));
            }
        }

        public static async Task<byte[]> GetZip(List<string> filePathList, CancellationToken cancellationToken = default)
        {
            var outputStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create))
            {
                await foreach (var file in GetFilesDataList(filePathList, cancellationToken))
                {
                    var zipEntry = zipArchive.CreateEntry(Path.GetFileName(file.path));
                    using (var zipStream = zipEntry.Open())
                        await zipStream.WriteAsync(file.data);
                }
            }

            return outputStream.ToArray();
        }
    }
}
