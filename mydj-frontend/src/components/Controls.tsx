interface ControlsProps {
  isPlaying: boolean
  shuffle: boolean
  repeat: boolean
  volume: number
  onPlay: () => void
  onPause: () => void
  onStop: () => void
  onShuffle: () => void
  onRepeat: () => void
  onPrev: () => void
  onNext: () => void
  onVolumeChange: (value: number) => void
}

const Controls = ({
  isPlaying,
  shuffle,
  repeat,
  volume,
  onPlay,
  onPause,
  onStop,
  onShuffle,
  onRepeat,
  onPrev,
  onNext,
  onVolumeChange
}: ControlsProps) => {
  return (
    <div className="controls-grid">
      <div className="transport-buttons">
        <button onClick={onShuffle} className={shuffle ? 'primary' : ''}>
          Shuffle
        </button>
        <button onClick={onPrev}>Prev</button>
        {isPlaying ? (
          <button onClick={onPause} className="primary">
            Pause
          </button>
        ) : (
          <button onClick={onPlay} className="primary">
            Play
          </button>
        )}
        <button onClick={onNext}>Next</button>
        <button onClick={onRepeat} className={repeat ? 'primary' : ''}>
          Repeat
        </button>
      </div>
      <div className="transport-buttons">
        <button onClick={onStop}>Stop</button>
      </div>
    </div>
  )
}

export default Controls
