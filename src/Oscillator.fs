module Oscillator

open System
open AudioTypes

let generate sampleRate frequency = 
    let delta = TWOPI * frequency / (float) sampleRate
    let gen theta = Some (Math.Sin theta, (theta + delta) % TWOPI)
    Seq.unfold gen 0.0

let sine f = makeStream 44100 (generate  44100 f)
