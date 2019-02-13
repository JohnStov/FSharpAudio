module Generators

open System

open AudioTypes

let silence = 
    let silenceGenerator state = Some (0.0, state)
    Seq.unfold silenceGenerator ()

let silenceStream = makeStream 44100 silence

let noise = 
    let rnd = new Random()
    let rescale value = (value * 2.0)  - 1.0
    let noiseGenerator state = 
        Some (rnd.NextDouble() |> rescale, state)

    Seq.unfold noiseGenerator ()

let noiseStream = makeStream 44100 noise