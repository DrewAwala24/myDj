interface VinylProps {
  isPlaying: boolean
  title: string
}

const Vinyl = ({ isPlaying, title }: VinylProps) => {
  return (
    <div className={`vinyl ${isPlaying ? 'spin' : ''}`} title={title}>
      <div className="vinyl-label">
        <span style={{ position: 'absolute', inset: 'calc(50% - 22px)' }} />
      </div>
    </div>
  )
}

export default Vinyl
