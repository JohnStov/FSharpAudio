module AudioOut

open AudioTypes
open NAudio.Wave
open System
open System.Runtime.InteropServices

type StreamProvider (streams: StreamBundle) =
    let sampleRate = 
        if streams.IsEmpty then
            invalidArg "streams" "At least one stream is required"
        else if streams |> List.exists (fun s -> s.sampleRate <> streams.[0].sampleRate) then
            invalidArg "streams" "All streams must have the same sample rate"
        else
            streams.[0].sampleRate

    let waveFormat = 
        WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, streams.Length)
    
    let enumerators = streams |> List.map (fun s -> s.data.GetEnumerator())

    let nextValue nStream = 
        if enumerators.[nStream].MoveNext() then enumerators.[nStream].Current else 0.0

    interface IWaveProvider with
        member __.WaveFormat with get() = waveFormat

        member __.Read (buffer, offset, count) =
            // wrap the buffer in a span and then cast that to a span of float
            let floatBuffer = MemoryMarshal.Cast<byte, float>(buffer.AsSpan(offset, count)) 

            for nSample in [0 .. floatBuffer.Length-1] do
                floatBuffer.[nSample] <- nextValue (nSample % streams.Length)

            // return the number of bytes written
            floatBuffer.Length * sizeof<float>
