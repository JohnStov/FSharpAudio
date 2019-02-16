module Mixer

open AudioTypes

let mix a b  =
    if a.sampleRate <> b.sampleRate then
            invalidArg "b" "All streams must have the same sample rate"
    {a with data = Seq.zip a.data b.data |> Seq.map (fun (a, b) -> (a + b / 2.0))}

let mix3 a b c = mix (mix a b) c

let fade factor stream =
    {stream with data = stream.data |> Seq.map (fun sample -> sample * factor)}