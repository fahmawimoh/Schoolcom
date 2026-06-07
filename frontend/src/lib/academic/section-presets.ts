export interface SectionPreset {
  nameEn: string;
  nameAr: string;
}

export const SECTION_PRESETS: SectionPreset[] = [
  { nameEn: "A", nameAr: "أ" },
  { nameEn: "B", nameAr: "ب" },
  { nameEn: "C", nameAr: "ج" },
  { nameEn: "D", nameAr: "د" },
  { nameEn: "E", nameAr: "هـ" },
  { nameEn: "F", nameAr: "و" },
];

export function normalizeSectionName(name: string): string {
  return name.trim().toLowerCase();
}
