# Set File Timestamp

A console application to edit file timestamps.

## Program Usage

```console
[options...] <files or folders...>
options:
  /F:type               file types to process (default: FDS)
                    F - file
                    D - directory
                    S - subfolders and files
  /S:options            timestamp types to set (default: CAW)
                    C - creation time
                    A - last access time
                    W - last write time
  /T:dateTime           date/time to use (default: now)
  /C:cultureNameOrLCID  culture used to parse and display timestamps (default: current)
  /P:searchPattern      file search filter (default: *.*)
  /R                    enable recursive folder search (default: disabled)
  /V                    enable verbose mode (default: disabled)
examples:
  1. overwrite dates of specified files:
     "README.md" "LICENSE.md"
  2. overwrite creation time of specified directory, its subfolders and files:
     /S:C "/T:5/11/2020 11:54:34 AM" "docs"
  3. overwrite dates of text files contained inside specified directory:
     /F:F /R "/P:*.txt" "docs"
  4. overwrite dates of subfolders contained inside specified directory:
     /F:S /R "docs"
  5. overwrite dates of specified directory:
     /F:D "docs"
```
