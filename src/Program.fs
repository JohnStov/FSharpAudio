// Learn more about F# at http://fsharp.org

open NAudio.Wave
open System.Threading

open AudioOut
open Generators

[<EntryPoint>]
let main argv =
    printfn "Starting playback"

    use out = new WaveOutEvent()
    [noiseStream] |> StreamProvider |> out.Init
    out.Play()
    Thread.Sleep(2000)
    out.Stop()

    printfn "Playback finished"

    0 // return an integer exit code
