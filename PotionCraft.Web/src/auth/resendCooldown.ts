/** Пауза между повторной отправкой кода в секундах (совпадает с ограничением сервера). */
export const RESEND_COOLDOWN_SECONDS = 60

/**
 * Возвращает, сколько целых секунд осталось до окончания паузы; 0, если пауза закончилась.
 * @param endsAt момент окончания паузы в миллисекундах (Date.now()-совместимый)
 * @param now текущий момент в миллисекундах
 */
export function getRemainingSeconds(endsAt: number, now: number): number {
  return Math.max(0, Math.ceil((endsAt - now) / 1000))
}
