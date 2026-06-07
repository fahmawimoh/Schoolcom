import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function pickLocalized(
  locale: string,
  en: string,
  ar: string
): string {
  return locale === "ar" ? ar || en : en || ar;
}
