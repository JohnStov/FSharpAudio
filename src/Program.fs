// Learn more about F# at http://fsharp.org


open System.Threading
open NAudio.Wave

open Mixer
open Oscillator
open AudioOut

let cMajor = mix3 (sine 261.63) (sine 329.63) (sine 392.00)

[<EntryPoint>]
let main argv =
    printfn "Starting playback"

    use out = new WaveOutEvent()
    [fade 0.3333 cMajor] |> StreamProvider |> out.Init
    [cMajor] |> StreamProvider |> out.Init

    out.Play()
    Thread.Sleep(5000)
    out.Stop()

    printfn "Playback finished"

    0 // return an integer exit code
