using System;
using System.Globalization;
using System.IO;

namespace SetFileTimestamp
{
	class Program
	{
		[Flags]
		enum FileTypes
		{
			None = 0,
			File = 1,
			Directory = 2,
			DirectoryContents = 4,
			All = File | Directory | DirectoryContents
		};

		[Flags]
		enum TimestampTypes
		{
			None = 0,
			CreationTime = 1,
			LastAccessTime = 2,
			LastWriteTime = 4,
			All = CreationTime | LastAccessTime | LastWriteTime
		};

		static int Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("[options...] <files or folders...>");
				Console.WriteLine("options:");
				Console.WriteLine("  /F:type               file types to process (default: FDS)");
				Console.WriteLine("                    F - file");
				Console.WriteLine("                    D - directory");
				Console.WriteLine("                    S - subfolders and files");
				Console.WriteLine("  /S:options            timestamp types to set (default: CAW)");
				Console.WriteLine("                    C - creation time");
				Console.WriteLine("                    A - last access time");
				Console.WriteLine("                    W - last write time");
				Console.WriteLine("  /T:dateTime           date/time to use (default: now)");
				Console.WriteLine("  /C:cultureNameOrLCID  culture used to parse and display timestamps (default: current)");
				Console.WriteLine("  /P:searchPattern      file search filter (default: *.*)");
				Console.WriteLine("  /R                    enable recursive folder search (default: disabled)");
				Console.WriteLine("  /V                    enable verbose mode (default: disabled)");
				Console.WriteLine("examples:");
				Console.WriteLine("  1. overwrite dates of specified files:");
				Console.WriteLine("     \"README.md\" \"LICENSE.md\"");
				Console.WriteLine("  2. overwrite creation time of specified directory, its subfolders and files:");
				Console.WriteLine("     /S:C \"/T:5/11/2020 11:54:34 AM\" \"docs\"");
				Console.WriteLine("  3. overwrite dates of text files contained inside specified directory:");
				Console.WriteLine("     /F:F /R \"/P:*.txt\" \"docs\"");
				Console.WriteLine("  4. overwrite dates of subfolders contained inside specified directory:");
				Console.WriteLine("     /F:S /R \"docs\"");
				Console.WriteLine("  5. overwrite dates of specified directory:");
				Console.WriteLine("     /F:D \"docs\"");
				return 0;
			}

			var fileTypes = FileTypes.All;
			var timestampTypes = TimestampTypes.All;
			var dateTime = DateTime.Now;
			var cultureInfo = CultureInfo.CurrentCulture;
			var searchPattern = "*.*";
			var recursiveSearch = false;
			var verbose = false;

			try
			{

				for (var i = 0; i < args.Length; ++i)
				{
					if (args[i].StartsWith("/F:"))
					{
						var arg0 = args[i].Substring("/F:".Length);

						fileTypes = FileTypes.None;

						foreach (var option in arg0)
						{
							if (option == 'C')
							{
								fileTypes |= FileTypes.File;
							}
							else if (option == 'D')
							{
								fileTypes |= FileTypes.Directory;
							}
							else if (option == 'S')
							{
								fileTypes |= FileTypes.DirectoryContents;
							}
							else
							{
								throw new Exception($"Unrecognized file type '{option}'.");
							}
						}
					}
					else if (args[i].StartsWith("/S:"))
					{
						var arg0 = args[i].Substring("/S:".Length);
						
						timestampTypes = TimestampTypes.None;

						foreach (var option in arg0)
						{
							if (option == 'C')
							{
								timestampTypes |= TimestampTypes.CreationTime;
							}
							else if (option == 'A')
							{
								timestampTypes |= TimestampTypes.LastAccessTime;
							}
							else if (option == 'W')
							{
								timestampTypes |= TimestampTypes.LastWriteTime;
							}
							else
							{
								throw new Exception($"Unrecognized timestamp type '{option}'.");
							}
						}
					}
					else if (args[i].StartsWith("/T:"))
					{
						var arg0 = args[i].Substring("/T:".Length);
						
						dateTime = DateTime.Parse(arg0, cultureInfo);

						if (verbose)
						{
							Console.WriteLine($"timestamp: {dateTime.ToString(cultureInfo.DateTimeFormat.FullDateTimePattern)}");
						}
					}
					else if (args[i].StartsWith("/C:"))
					{
						var arg0 = args[i].Substring("/C:".Length);

						if (int.TryParse(arg0, out var localeID))
						{
							cultureInfo = CultureInfo.GetCultureInfo(localeID);
						}
						else
						{
							cultureInfo = CultureInfo.GetCultureInfo(arg0);
						}

						if (verbose)
						{
							Console.WriteLine($"culture: {cultureInfo.DisplayName}");
						}
					}
					else if (args[i].StartsWith("/P:"))
					{
						var arg0 = args[i].Substring("/P:".Length);

						searchPattern = arg0;
					}
					else if (args[i].Equals("/R"))
					{
						recursiveSearch = true;
					}
					else if (args[i].Equals("/V"))
					{
						verbose = true;
					}
					else if (Directory.Exists(args[i]))
					{
						var dirInfo = new DirectoryInfo(args[i]);
						var searchOption = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

						if (fileTypes.HasFlag(FileTypes.File))
						{
							foreach (var fileInfo in dirInfo.EnumerateFiles(searchPattern, searchOption))
							{
								SetFileTimestamps(fileInfo, timestampTypes, dateTime, cultureInfo, verbose);
							}
						}

						if (fileTypes.HasFlag(FileTypes.DirectoryContents))
						{
							foreach (var subDirInfo in dirInfo.EnumerateDirectories(searchPattern, searchOption))
							{
								SetDirectoryTimestamps(subDirInfo, timestampTypes, dateTime, cultureInfo, verbose);
							}
						}

						if (fileTypes.HasFlag(FileTypes.Directory))
						{
							SetDirectoryTimestamps(dirInfo, timestampTypes, dateTime, cultureInfo, verbose);
						}
					}
					else if (File.Exists(args[i]))
					{
						if (fileTypes.HasFlag(FileTypes.File))
						{
							SetFileTimestamps(args[i], timestampTypes, dateTime, cultureInfo, verbose);
						}
					}
					else
					{
						throw new Exception($"Unrecognized argument '{args[i]}' at index {i.ToString(CultureInfo.InvariantCulture)}.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				return 1;
			}

			return 0;
		}

		private static void SetFileTimestamps(FileInfo fileInfo, TimestampTypes timestampTypes, DateTime dateTime, CultureInfo cultureInfo, bool verbose)
		{
			SetFileTimestamps(fileInfo.FullName, timestampTypes, dateTime, cultureInfo, verbose);
		}

		private static void SetFileTimestamps(string fileName, TimestampTypes timestampTypes, DateTime dateTime, CultureInfo cultureInfo, bool verbose)
		{
			Log(fileTypePrefix: "F", fileName, timestampTypes, dateTime, cultureInfo, verbose);

			if (timestampTypes.HasFlag(TimestampTypes.CreationTime))
			{
				File.SetCreationTime(fileName, dateTime);
			}

			if (timestampTypes.HasFlag(TimestampTypes.LastAccessTime))
			{
				File.SetLastAccessTime(fileName, dateTime);
			}

			if (timestampTypes.HasFlag(TimestampTypes.LastWriteTime))
			{
				File.SetLastWriteTime(fileName, dateTime);
			}
		}

		private static void SetDirectoryTimestamps(DirectoryInfo dirInfo, TimestampTypes timestampTypes, DateTime dateTime, CultureInfo cultureInfo, bool verbose)
		{
			Log(fileTypePrefix: "D", dirInfo.FullName, timestampTypes, dateTime, cultureInfo, verbose);

			if (timestampTypes.HasFlag(TimestampTypes.CreationTime))
			{
				Directory.SetCreationTime(dirInfo.FullName, dateTime);
			}

			if (timestampTypes.HasFlag(TimestampTypes.LastAccessTime))
			{
				Directory.SetLastAccessTime(dirInfo.FullName, dateTime);
			}

			if (timestampTypes.HasFlag(TimestampTypes.LastWriteTime))
			{
				Directory.SetLastWriteTime(dirInfo.FullName, dateTime);
			}
		}

		private static void Log(string fileTypePrefix, string fileName, TimestampTypes timestampTypes, DateTime dateTime, CultureInfo cultureInfo, bool verbose)
		{
			if (!verbose)
			{
				return;
			}

			Console.WriteLine($"{dateTime.ToString(cultureInfo.DateTimeFormat.FullDateTimePattern)}  {fileTypePrefix}  {fileName}");
		}
	}
}
