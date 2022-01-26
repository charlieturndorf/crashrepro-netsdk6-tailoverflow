#r "packages/FSharp.Core/lib/netstandard2.0/FSharp.Core.dll"
#r "packages/MathNet.Numerics/lib/netstandard2.0/MathNet.Numerics.dll"
#r "packages/MathNet.Numerics.FSharp/lib/netstandard2.0/MathNet.Numerics.FSharp.dll"

#r "bin/CrashRepro.TailOverflow.dll"

open CrashRepro.TryRepro

tryRepro ()