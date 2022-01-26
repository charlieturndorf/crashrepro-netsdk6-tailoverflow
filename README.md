# Crash Repro: .NET SDK 6 -- Tail Overflow
A minimal repro of an F# bad tailcall overflow introduced by .NET SDK 6

# Repro Instructions (for Windows)
From the repo root on the command line, in order:
0. `init.cmd`
   - You only need to run this once to bootstrap Paket
1. `build.cmd`
2. `dotnet fsi` (or host fsi.exe in your terminal of choice)
3. `#load "Repro.fsx";;`
   - A stack overflow will tear down the fsi process

# Does this code ever work?
Yes. To prove this is an upstream regression:
- In global.json, set sdk version to `5.0.404`
- Follow the repro instructions above from step 1
- Everything is ok (no overflow)

# Does this work in release mode?
Yes.
- In build.cmd, change `-c Debug` to `-c Release`
- Revert sdk version in global.json (if you changed it) to `6.0.101`
- Follow the repro instructions above from step 1
- Everything is ok (no overflow)

# Notes
- Built with VS Code on Windows 10
- Targets netstandard2.1

# License
- This repo is MIT licensed
- Please note that src/Rng.fs contains an excerpt adapted from FsCheck:
  - https://github.com/fscheck/FsCheck/blob/master/src/FsCheck/Random.fs
  - https://github.com/fscheck/FsCheck/blob/master/License.txt (BSD 3-Clause)