import { z } from "zod";

export const emailLoginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(8, "Password must be at least 8 characters"),
  rememberMe: z.boolean().optional(),
});

export const phoneLoginSchema = z.object({
  phone: z
    .string()
    .min(10, "Invalid phone number")
    .regex(/^\+?[0-9\s-]+$/, "Invalid phone format"),
});

export const otpVerifySchema = z.object({
  otp: z
    .string()
    .length(6, "OTP must be 6 digits")
    .regex(/^\d+$/, "OTP must contain only digits"),
});

export type EmailLoginForm = z.infer<typeof emailLoginSchema>;
export type PhoneLoginForm = z.infer<typeof phoneLoginSchema>;
export type OtpVerifyForm = z.infer<typeof otpVerifySchema>;

export const whatsAppSendSchema = z.object({
  phone: z.string().min(10, "Invalid phone number"),
  message: z.string().min(1, "Message is required").max(4096),
});

export type WhatsAppSendForm = z.infer<typeof whatsAppSendSchema>;
