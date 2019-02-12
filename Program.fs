// Learn more about F# at http://fsharp.org

open AudioTypes
open AudioOut
open NAudio.Wave
open System.Threading

let silenceGenerator state = Some (0.0, state)
let silence = Seq.unfold silenceGenerator ()
let stream = {data = silence; sampleRate = 44100}
let bundle = [stream]

[<EntryPoint>]
let main argv =
    printfn "Starting playback"

    let out = new WaveOutEvent()
    out.Init(new StreamProvider(bundle))
    out.Play()
    Thread.Sleep(1000)
    out.Stop()

    printfn "Playback finished"

    0 // return an integer exit code
