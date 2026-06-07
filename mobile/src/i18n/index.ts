import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import { I18nManager } from "react-native";
import * as Localization from "expo-localization";
import en from "./en.json";
import ar from "./ar.json";

export type AppLocale = "en" | "ar";

const resources = {
  en: { translation: en },
  ar: { translation: ar },
};

const deviceLocale = Localization.getLocales()[0]?.languageCode ?? "en";
const initialLocale: AppLocale = deviceLocale === "ar" ? "ar" : "en";

export function applyRtl(locale: AppLocale) {
  const isRtl = locale === "ar";
  if (I18nManager.isRTL !== isRtl) {
    I18nManager.allowRTL(isRtl);
    I18nManager.forceRTL(isRtl);
  }
}

applyRtl(initialLocale);

i18n.use(initReactI18next).init({
  resources,
  lng: initialLocale,
  fallbackLng: "en",
  interpolation: { escapeValue: false },
  compatibilityJSON: "v4",
});

export function setAppLocale(locale: AppLocale) {
  applyRtl(locale);
  return i18n.changeLanguage(locale);
}

export default i18n;
