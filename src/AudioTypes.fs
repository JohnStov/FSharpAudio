module AudioTypes

type AudioStream = { data : float seq; sampleRate : int }

type StreamBundle = AudioStream list

let makeStream sampleRate stream = {data = stream; sampleRate = sampleRate}

