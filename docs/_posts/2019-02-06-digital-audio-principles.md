---
layout: post
title:  "Digital Audio Principles"
date:   2019-02-06 17:00:00 +0000
categories: fsharp audio intro
---
# Digital Audio Principles

Before we dive into code, we need to understand some of the principles that underpin digital audio.

## Sound waves

Sound is caused by air molecules moving backwards and forwards in a regular way. This movement of the air moves your eardrum, which stimulates your aural nerve endings. This in turn causes something to happen in your brain, and you perceive sound. 

We can capture sound by using a microphone, which converts the movement of air into a change in an electrical signal. This graph shows plots this changing electrical signal (vertical/y-axis) against time (horizontal/x-axis)

![Waveform](images/waveform.png)

We can make 2 observations here:
    1. _Loudness_ corresponds to the magnitude of the signal in the y-axis
    2. _Pitch_ corresponds inversely to the distance between peaks in the x-axis. This is clearer if we simplify our waveform to a _sine wave_ ![Sinewave](images/sine.png)

## Digital recording

We can make digital recordings by _sampling_ our signal at regular time intervals - that is, measuring the instantantaneous value of the signal at that point in time. We can than convert that into a number on a linear scale (16-bit resolution would use a scale from -32767 to +32767), and store the resulting sequence of values. This is Analogue to Digital _(A/D)_ conversion.

To play back the recording we can convert the numbers back into voltages at the same rate that we recorded them, and smooth the resulting signal. This is Digital to Analogue _(D/A)_ conversion.

Furthermore, if we can replay a previously recorded sound, we generate entirely new sounds just by generating sequences of numbers and passing those to the D/A converter at the correct rate.

## Audio signals as code

As we have seen, we can model an audio signal as a sequence of numbers. We usually store these as floating point values to make mathematical manipulation easier. In F#, We could use a `float list` or a `float array`, but that would force us to pre-compute all our signals before we can play them back, and we might run out of memory.

Instead we will use `float seq` (`IEnumerable<float>` for C# programmers), so that we can evaluate values lazily with `yield`. This will save us memory and open up the opportunity for live performance by changing our generated signal on the fly.

In order to play back a list of samples accurately, we also need to know the _sample rate_. We could configure this globally, but bundling this in with the data stream will give us more flexibility later on.

That leads to the following type declaration:

``` fsharp
type AudioStream = { sampleRate : int; samples : float seq }
```
This is a mono stream. If we want stereo, or more channels, we need to bundle a number of channels together so that we play back the samples in lock-step.

``` fsharp
type AudioBundle = AudioStream list
```
