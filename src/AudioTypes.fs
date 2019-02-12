module AudioTypes

type AudioStream = { data : float seq; sampleRate : int }
type StreamBundle = AudioStream list