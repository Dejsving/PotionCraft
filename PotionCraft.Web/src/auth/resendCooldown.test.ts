import { describe, expect, it } from 'vitest'
import { getRemainingSeconds, RESEND_COOLDOWN_SECONDS } from './resendCooldown'

describe('getRemainingSeconds', () => {
  it('counts whole seconds left until the end of the cooldown', () => {
    const start = 1_000_000
    const endsAt = start + RESEND_COOLDOWN_SECONDS * 1000
    expect(getRemainingSeconds(endsAt, start)).toBe(60)
    expect(getRemainingSeconds(endsAt, start + 18_000)).toBe(42)
    expect(getRemainingSeconds(endsAt, start + 59_500)).toBe(1)
  })

  it('returns 0 once the cooldown is over', () => {
    const endsAt = 5_000
    expect(getRemainingSeconds(endsAt, 5_000)).toBe(0)
    expect(getRemainingSeconds(endsAt, 9_000)).toBe(0)
  })
})
