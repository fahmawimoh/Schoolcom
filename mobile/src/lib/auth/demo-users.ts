export type UserRole =
  | "SuperAdmin"
  | "SchoolAdmin"
  | "Teacher"
  | "Parent";

export interface DemoUser {
  id: string;
  email: string;
  phone: string;
  password: string;
  firstName: string;
  lastName: string;
  roles: UserRole[];
  preferredLanguage: "en" | "ar";
  tenantId: string;
}

export const DEMO_USERS: DemoUser[] = [
  {
    id: "22222222-2222-2222-2222-222222222222",
    email: "admin@demo.school",
    phone: "+966501234567",
    password: "Admin@123456",
    firstName: "School",
    lastName: "Admin",
    roles: ["SchoolAdmin"],
    preferredLanguage: "en",
    tenantId: "11111111-1111-1111-1111-111111111111",
  },
  {
    id: "33333333-3333-3333-3333-333333333333",
    email: "parent@demo.school",
    phone: "+966509876543",
    password: "Admin@123456",
    firstName: "Ahmed",
    lastName: "Al-Rashid",
    roles: ["Parent"],
    preferredLanguage: "ar",
    tenantId: "11111111-1111-1111-1111-111111111111",
  },
  {
    id: "44444444-4444-4444-4444-444444444444",
    email: "teacher@demo.school",
    phone: "+966507654321",
    password: "Admin@123456",
    firstName: "Sarah",
    lastName: "Johnson",
    roles: ["Teacher"],
    preferredLanguage: "en",
    tenantId: "11111111-1111-1111-1111-111111111111",
  },
];

export function findUserByEmail(email: string): DemoUser | undefined {
  return DEMO_USERS.find(
    (u) => u.email.toLowerCase() === email.toLowerCase()
  );
}

export function findUserByPhone(phone: string): DemoUser | undefined {
  const normalized = phone.replace(/[\s-]/g, "");
  return DEMO_USERS.find(
    (u) => u.phone.replace(/[\s-]/g, "") === normalized
  );
}

export function findUserByIdentifier(identifier: string): DemoUser | undefined {
  return findUserByEmail(identifier) ?? findUserByPhone(identifier);
}

export function toAuthUser(user: DemoUser) {
  return {
    id: user.id,
    email: user.email,
    phone: user.phone,
    firstName: user.firstName,
    lastName: user.lastName,
    roles: user.roles,
    preferredLanguage: user.preferredLanguage,
    tenantId: user.tenantId,
  };
}

export function createDevToken(user: DemoUser): string {
  return `dev.${user.id}.${Date.now()}`;
}

const otpStore = new Map<string, { code: string; expiresAt: number }>();

export function storeOtp(identifier: string, code: string, ttlMs = 300_000) {
  const key = identifier.replace(/[\s-]/g, "");
  otpStore.set(key, { code, expiresAt: Date.now() + ttlMs });
}

export function verifyOtp(identifier: string, code: string): boolean {
  const key = identifier.replace(/[\s-]/g, "");
  const entry = otpStore.get(key);
  if (!entry) return false;
  if (Date.now() > entry.expiresAt) {
    otpStore.delete(key);
    return false;
  }
  if (entry.code !== code) return false;
  otpStore.delete(key);
  return true;
}

export function generateOtp(): string {
  return String(Math.floor(100000 + Math.random() * 900000));
}

export function loginWithEmail(email: string, password: string) {
  const user = findUserByEmail(email);
  if (!user || user.password !== password) {
    throw new Error("Invalid email or password");
  }
  return {
    accessToken: createDevToken(user),
    expiresIn: 900,
    user: toAuthUser(user),
  };
}

export function requestOtp(identifier: string) {
  const user = findUserByIdentifier(identifier);
  if (!user) throw new Error("User not found");
  const code = generateOtp();
  storeOtp(identifier, code);
  return { message: "Verification code sent", expiresInSeconds: 300, devCode: code };
}

export function verifyOtpLogin(identifier: string, code: string) {
  if (!verifyOtp(identifier, code)) {
    throw new Error("Invalid or expired verification code");
  }
  const user = findUserByIdentifier(identifier);
  if (!user) throw new Error("User not found");
  return {
    accessToken: createDevToken(user),
    expiresIn: 900,
    user: toAuthUser(user),
  };
}
