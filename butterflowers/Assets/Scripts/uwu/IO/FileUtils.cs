using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace uwu.IO
{
	public static class FileUtils
	{
		public static void EnsureDirectory(string dir, bool hidden = false)
		{
			DirectoryInfo dirInfo = null;

			if (!Directory.Exists(dir))
				dirInfo = Directory.CreateDirectory(dir);
			else
				dirInfo = new DirectoryInfo(dir);

			
			if(hidden) 
			{
				dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			}
		}

		public static string GetDirectory(string path)
		{
			var backslashInd = path.LastIndexOf('/');
			var dir = path.Substring(0, path.Length - backslashInd);

			return dir;
		}

		// Snippet for async/await file move, source (https://stackoverflow.com/questions/14162983/system-io-file-move-how-to-wait-for-move-completion)
		public static async Task MoveFile(string source, string dest)
		{
			try {
				using (var sourceStream = File.Open(source, FileMode.Open)) {
					using (var destinationStream = File.Create(dest)) {
						await sourceStream.CopyToAsync(destinationStream);

						sourceStream.Close();
						File.Delete(source);
					}
				}
			}
			catch (IOException ioex) {
				Debug.Log(ioex.Message);
			}
			catch (Exception ex) {
				Debug.Log(ex.Message);
			}
		}
	}
}