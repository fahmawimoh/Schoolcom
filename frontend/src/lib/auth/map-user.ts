import type { AuthUser, UserRole } from "@/lib/api/client";

export function mapLoginUserToAuthUser(user: {
  id: string;
  email?: string | null;
  phone?: string | null;
  firstName: string;
  lastName: string;
  roles: string[];
  preferredLanguage: string;
  tenantId: string;
}): AuthUser {
  return {
    id: String(user.id),
    email: user.email ?? undefined,
    phone: user.phone ?? undefined,
    firstName: user.firstName,
    lastName: user.lastName,
    roles: user.roles as UserRole[],
    preferredLanguage: user.preferredLanguage === "ar" ? "ar" : "en",
    tenantId: user.tenantId,
  };
}
