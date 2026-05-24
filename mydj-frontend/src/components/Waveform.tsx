interface WaveformProps {
  bars: number[]
}

const Waveform = ({ bars }: WaveformProps) => {
  return (
    <div className="waveform-track" aria-label="waveform visualization">
      {bars.map((height, index) => (
        <span key={index} className="waveform-bar" style={{ height: `${height}%` }} />
      ))}
    </div>
  )
}

export default Waveform
