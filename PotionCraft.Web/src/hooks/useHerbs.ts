import { useQuery } from '@tanstack/react-query'
import { getHerbs } from '../api/herbs'

/** Загружает и кэширует общий справочник трав для страниц приложения. */
export function useHerbs() {
  return useQuery({
    queryKey: ['herbs'],
    queryFn: getHerbs,
  })
}