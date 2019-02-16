---
layout: post
title:  "Oscillators"
date:   2019-02-14 18:00:00 +0000
categories: fsharp audio oscillators
---

# Oscillators

Now that we can make random noise, it's time to try to create a more structured sound. The simplest waveform we can create is a sine wave. This is the shape created by constant circular motion if we observe only the vertical component. We can use the `Math.Sin` function to calculate the value for any angle within a circle (don't forget that the Math library trigonometic functions work in _radians_ - there are 2π radians in a circle).

A sine wave is a repetitive signal, so it has a _frequency_ - the number of times that the waveform repeats every second. We can calculate the change in _phase angle_ (I call this _delta_) between any two samples for an output frequency `f` and a sample rate `S` as `(2πf)/S`. To calculate the next sample value in sequence, we take the previous phase value, add delta and calculate the sin. This gives us the following function:

``` fsharp
let TWOPI = 2.0 * Math.PI

let generate sampleRate frequency = 
    let delta = TWOPI * frequency / (float) sampleRate
    
    let gen theta = Some (Math.Sin theta, (theta + delta) % TWOPI)
    
    Seq.unfold gen 0.0
```
There are a few things to note here. Firstly we calculate 2π and store it as a constant to save a multiply operation. Secondly, the value of delta never changes, so we can calculate it once when we create the stream. Thirldy, we use unfold again, this time using our phase angle (`theta`) as the state that we thread through the calculation, starting with`0.0`. Every time we require a new sample value, we calculate the sine of the current angle and then add `delta` to that angle. We also take the modulus with 2π to ensure that we never get an overflow.  

We can use this to create a 440Hz (concert A) sinewave stream:

``` fsharp
let sine440 = makeStream 44100 (generate 440 44100)
```

Even better, we can create a function to generate a sinewave of any frequency:

``` fsharp
let sine f = makeStream 44100 (generate 44100 f)
```

and we can use that as the input to our `StreamProvider`:

``` fsharp
...
    [sine 440.0] |> StreamProvider |> out.Init
...
```

## Mixing

Now we can produce a single tone, let's try producing a chord. To mix two sample streams together, we need to add the samples in each stream pairwise, Since this means our potential output range now goes from `-1.0` to `1.0`, we also need to halve the resulting value to ensure we exceed our output range.

Since we are adding `AudioStream`s we need to do a bit of work to get hold of the `seq` inside it:

``` fsharp
let mix a b  =
    if a.sampleRate <> b.sampleRate then
            invalidArg "b" "All streams must have the same sample rate"
    {a with data =
         Seq.zip a.data b.data 
         |> Seq.map (fun (a, b) -> (a + b / 2.0))}
```

We take the two data streams and `zip` them to get a sequence of tuples of the pairwise values. `map` then applies a function to each tuple in turn, adding them and dividing by `2.0`

We can define a `mix3` function in terms of `mix` to mix 3 streams together:

```fsharp
let mix3 a b c = mix (mix a b) c
```
The C Major chord consists of the notes of C E and G. These have frequencies of 261.63 Hz,  329.63 Hz and 392.00 Hz respectively. We can define this using `mix3`:

``` fsharp
let cMajor = mix3 (sine 261.63) (sine 329.63) (sine 392.00)
```

We can also control the overall level or volume of a stream by my mltiplying by a fraction (note that our ears are not linear in this respect, so that halving the level will not reduce the perceived loudness by half). Lets try reducing the level by two thirds:

``` fsharp
let fade factor stream =
    {stream with data = stream.data |> Seq.map (fun sample -> sample * factor)}
```

and then use that to generate the output:

``` fsharp
...
    [fade 0.3333 cMajor] |> StreamProvider |> out.Init
...
```
