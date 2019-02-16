---
layout: post
title:  "Oscillators"
date:   2019-02-14 18:00:00 +0000
categories: fsharp audio oscillators
---

# Oscillators

Now that we can make random noise, it's time to try to create a more structured sound. The simplest waveform we can create is a sine wave. This is the shape created by constant circular motion if we observe only the vertical component. We can use the `Math.Sin` function to calculate the value for any angle within a circle (don't forget that the Math library trigonometic functions work in _radians_ - there are 2π radians in a circle).

A sine wave is a repetitive signal, so it has a _frequency_ - the number of times that the waveform repeats every second. We can calculate the change in _phase angle_ (I call this _delta_) between any two samples for an output frequency `f` and a sample rate `S` as `(2πf)/S`. To calculate the next sample value in sequence, we take the previous phase value, add _delta_ and calculate the sin. This gives us the following function:

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