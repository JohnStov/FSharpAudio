---
layout: post
title:  "Digital Audio Output with NAudio"
date:   2019-02-11 18:00:00 +0000
categories: fsharp audio intro
---

# Audio output with NAudio

Mark Heath has written the fabulous [NAudio](https://github.com/naudio/NAudio) library that provides a uniform interface around several different Windows audio APIs, and also provides support for the MIDI protocol. 

In order to output a sequence of values as sound, we have to implement this interface:

```csharp
public interface IWaveProvider
{
    WaveFormat WaveFormat { get; }

    int Read(byte[] buffer, int offset, int count);
}
```

Audio output in NAudio is callback-based. At regular intervals, NAudio will call our implementation of `Read(...)`, and passes us a `buffer` that it expects us to fill with data. `offset` and `count` are provided so that NAudio can request a partial buffer if it needs it. Filling a buffer rather than requesting individual values allows us to tune the performance of the playback system by adjusting the buffer size. 

We are also expected to provide a `WaveFormat` object which tells NAudio exactly how to interpret the data that we have inserted into the buffer.  We will use an `IeeeFloatWaveFormat`, because we want to use 32-bit IEEE floating point values: this is the `float` type in F#.

The implementation looks like this:
```fsharp
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
        if enumerators.[nStream].MoveNext() then 
            enumerators.[nStream].Current 
            else 0.0
        |> (float32)

    interface IWaveProvider with
        member __.WaveFormat with get() = waveFormat

        member __.Read (buffer, offset, count) =
            // wrap the buffer in a span and then cast that to a span of float
            let floatBuffer = MemoryMarshal.Cast<byte, float32>(buffer.AsSpan(offset, count)) 

            for nSample in [0 .. floatBuffer.Length-1] do
                floatBuffer.[nSample] <- nextValue (nSample % streams.Length)

            // return the number of bytes written
            floatBuffer.Length * sizeof<float32>
```

This is a class type with a constructor that takes one argument - a bundle of audio streams. We check that we have at least one stream and that all the streams have the same sample rate, and use this value to set the sample rate of the system.

Next we create a WaveFormat object for the number of streams we have been given, using the sample rate we found earlier. 

After this we get all the enumerators for the streams we are going to output. We need to maintain these between calls to `Read()`, or we will just continually replay the first few sample in each stream.

`nextValue()` is a helper function to get the next value from a specific stream. It adds a bit of safety by returning `0.0` if we reach the end of a stream. It also casts our `float` value (an 8 byte floating point value) to a `float32` (a 4 byte floating point value) which is what NAudio is expecting.

Finally we implement the `IWaveProvider` interface. The `WaveFormat` getter just returns the `WaveFormat` object we created earlier. `Span` makes implementing an efficient `Read()` method very strainghtforward - we can cast our buffer of bytes into a Span of floats and write values directly into the Span. Earlier implementations of this required a good deal of array copying, with resulting inefficiencies. 

Note that NAudio expects samples values for individual streams to be interleaved within the buffer, and that we return the number of bytes that were actually inserted.

Now we have our wave provider, we can create a simple console application that uses it. But before we do that we need to create a stream to output.  The simplest thing we can do is to create a sequence of zero values - this will produce silence, but will verify that our provider works

``` fsharp
let silence = 
    let silenceGenerator state = Some (0.0, state)
    Seq.unfold silenceGenerator ()
```

A quick note for those unfamiliar with `unfold()`. I will use this function a lot in later posts, so it's worth making sure you understand it now. 

This higher order function has this signature: `('State -> ('T * 'State) option) -> 'State -> seq<'T>`. It takes a generator function and an initial state, and generates a list of values. The generator takes the current state, and returns an option of a pair of the value to emit and the new state. If the resulting option value is `None`, the sequence ends. In this case, we want an endless sequence of `0.0`, so the state is irrelevant. In this case I simply pass the state through to the result, and use `unit` as my inital state.

We can also provide a helper function to wrap a `float seq` in an `AudioStream`

``` fsharp
let makeStream sampleRate stream = {data = stream; sampleRate = sampleRate}
```

And so we can create a silence stream

``` fsharp
let silenceStream = makeStream 44100 silence
```

Now that we have an `AudioStream`, we can create our output object. We will use Naudio's `WaveOutEvent` type, because it will work in a console application.

``` fsharp
[<EntryPoint>]
let main argv =
    printfn "Starting playback"

    use out = new WaveOutEvent()
    [silenceStream] |> StreamProvider |> out.Init
    out.Play()
    Thread.Sleep(1000)
    out.Stop()

    printfn "Playback finished"

    0 // return an integer exit code
```

As you can see, we create the output object and then pass it a StreamProvider. Note the `use` statement: `WaveOutEvent` implents `IDisposable`, and this makes sure that the object is properly disposed when we leave scope.

Then we start playback, wait for one second and then stop playback. Although this produces no sound, if you put a breakpoint on the `silenceGenerator` function, you will see that it gets called repeatedly.

Finally, we can create a simple white noise stream. This just creates a sequence of random values between `1.0` and `-1.0`.

``` fsharp
let noise = 
    let rnd = new Random()
    let rescale value = (value * 2.0)  - 1.0
    let noiseGenerator state = 
        Some (rnd.NextDouble() |> rescale, state)

    Seq.unfold noiseGenerator ()

let noiseStream = makeStream 44100 noise
```

This creates a new Random number generator and then repeatedly gets the next `double` value from it. These values are in the range `0.0` to `1.0`, so the `rescale` is used to fit it in the range `1.0` and `-1.0`.  If you replace `silenceStream` with `noiseStream` in the main function above, you should hear 2 seconds of white noise when you run the program.