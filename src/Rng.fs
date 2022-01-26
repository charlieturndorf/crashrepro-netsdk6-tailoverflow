module CrashRepro.Rng

open MathNet.Numerics.Random



// The following code is adapted from FsCheck's Random at
// https://github.com/fscheck/FsCheck/blob/master/src/FsCheck/Random.fs
// https://github.com/fscheck/FsCheck/blob/master/License.txt (BSD 3-Clause)

/// A seed representing a point in an immutable forward stream of rngs
type RngSeed = Seed of int * int

/// Provides functions for working with streams of Rngs.
///
/// Generates random numbers based on splitting seeds.
/// Based on Hugs' Random implementation.
[<RequireQualifiedAccess>]
module Rng =

    open System

    // Kurt Schelfthout:
    //"Haskell has mod,quot, en divMod. .NET has DivRem, % and /.
    // Haskell              | F#
    //---------------------------------
    // rem                  | %
    // mod                  | ?
    // quot                 | /
    // div                  | ?
    // divMod               | ?
    // quotRem              | divRem
    //
    //since the implementation uses divMod and mod, we need to
    //reimplement these. fortunately that's fairly easy"
    let private divMod64 (n:int64) d =
        let q = n / d
        let r = n % d
        if (Math.Sign(r) = -Math.Sign(d)) then (q-1L,r+d) else (q,r)
    let private hMod64 n d =
        let _,r = divMod64 n d
        r
    let private q1,q2 = 53668,52774
    let private a1,a2 = 40014,40692
    let private r1,r2 = 12211,3791
    let private m1,m2 = 2147483563,2147483399

    /// Creates an rng seed from the given long
    let inline private ofInt64 seed =
        let s = if seed < 0L then -seed else seed
        let (q, s1) = divMod64 s (int64 (m1-1))  //2147483562L
        let s2 = hMod64 q (int64 (m2-1)) //2147483398L
        Seed (int (s1+1L), int (s2+1L))

    /// Creates a random rng seed
    let seed () : RngSeed =
        Random.xorshift().NextFullRangeInt64() |> ofInt64


    /// Splits an int from an rng seed
    let inline private next (Seed (s1, s2)) =
        let k = s1 / q1
        let s1' = a1 * (s1 - k * q1) - k * r1
        let s1'' = if (s1' < 0) then s1 + m1 else s1'
        let k' = s2 / q2
        let s2' = a2 * (s2 - k' * q2) - k' * r2
        let s2'' = if s2' < 0 then s2' + m2 else s2'
        let z = s1'' - s2''
        let z' = if z < 1 then z + m1 - 1 else z
        (z', Seed (s1'', s2''))


    /// Splits an rng seed into two streams
    let split ((Seed (s1, s2)) as seed) : RngSeed * RngSeed =
        let new_s1 = if s1 = (m1-1) then 1 else s1 + 1
        let new_s2 = if s2 = 1 then (m2-1) else s2 - 1
        let (Seed (t1, t2)) = snd (next seed)
        let left = Seed (t1, new_s2)
        let right = Seed (new_s1, t2)
        (left, right)

    // Code adapted from FsCheck ends here



    /// Splits an rng seed into n + 1 streams
    let splitN n (seed:RngSeed) : RngSeed list * RngSeed =
        let rec _split n (acc, stream) =
            if n < 1 then (acc, stream)
            else
                split stream |> fun (stream, next) ->
                    _split (n-1) (next::acc, stream)
        split seed |> fun (branch, trunk) ->
            _split n ([], branch) |> fst, trunk