interface EQKnobsProps {
  values: Record<string, number>
}

const EQKnobs = ({ values }: EQKnobsProps) => {
  const entries = Object.entries(values)

  return (
    <>
      {entries.map(([label, value]) => {
        const rotation = (value / 100) * 240 - 120
        return (
          <div key={label} className="knob-card">
            <div className="knob" style={{ transform: `rotate(${rotation}deg)` }} />
            <div className="knob-label">{label}</div>
          </div>
        )
      })}
    </>
  )
}

export default EQKnobs
