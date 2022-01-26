module CrashRepro.TryRepro

open System.IO
open Rng

/// Path where logs should be written
let logDir =
    try
        let logDir = Path.Combine(__SOURCE_DIRECTORY__, "logs")
        Directory.CreateDirectory(logDir) |> ignore
        Some logDir
    with ex ->
        printfn "ERROR: Couldn't create the log directory!\n%s" (ex.ToString())
        None

/// Logs an error message to a new txt log file
let logError errStr =
    /// If the log file can't be created, just dump to stdout
    let dumpErr () =
        printfn "----> Reproduced the issue! <----"
        printfn "However, a log file couldn't be written; about to dump the error to stdout..."
        printfn "Dumping error to stdout in 5 seconds..."
        System.Threading.Thread.Sleep(5000)
        printfn "%s" errStr
    // Do we have a log directory?
    match logDir with
    | Some logDir ->
        try
            // Create a fresh log file
            let logName =
                System.DateTime.UtcNow.ToFileTimeUtc().ToString()
                |> sprintf "repro-log-%s.txt"
            use fstream = File.Create(Path.Combine(logDir, logName))
            use writer = new StreamWriter(fstream, System.Text.Encoding.UTF8)
            // Write error details to the log
            writer.WriteLine("Reproduced the issue, see below for exception details:\n")
            writer.WriteLine(errStr)
            printfn "----> Reproduced the issue! Logged to logs/%s <----" logName
        with ex ->
            printfn "Error writing log file:\n%s\n" (ex.ToString())
            dumpErr ()
    | None -> dumpErr ()


/// Tries to reproduce an exception that occurs when built with .NET SDK 6 in debug mode
/// (will not occur in release mode, or when built with .NET SDK 5 in any mode)
let tryRepro () =
    try
        printfn "Warning: a stack overflow may be incoming!"
        printfn "Attempting to reproduce in 3 seconds...\n"
        System.Threading.Thread.Sleep(3000)

        // How long a list should we generate?
        let targetNum = 10_000

        // Get a first seed
        let firstSeed = Rng.seed ()

        // Split off the desired number of seeds
        let seedList, _ = Rng.splitN targetNum firstSeed
        printfn "----> Did not reproduce the issue. Everything worked fine!"
        ()
    with ex ->
        // Issue reproduced! But if this is a stack overflow, it won't be handled...
        logError <| ex.ToString()