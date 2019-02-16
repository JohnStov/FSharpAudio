module AudioTypes

open System

type AudioStream = { data : float seq; sampleRate : int }

type StreamBundle = AudioStream list

let makeStream sampleRate stream = {data = stream; sampleRate = sampleRate}

let TWOPI = 2.0 * Math.PI
