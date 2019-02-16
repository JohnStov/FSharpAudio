// Learn more about F# at http://fsharp.org

open System.Threading
open NAudio.Wave

open Oscillator
open AudioOut

[<EntryPoint>]
let main argv =
    printfn "Starting playback"

    use out = new WaveOutEvent()
    [sine 440.0] |> StreamProvider |> out.Init

    out.Play()
    Thread.Sleep(5000)
    out.Stop()

    printfn "Playback finished"

    0 // return an integer exit code
