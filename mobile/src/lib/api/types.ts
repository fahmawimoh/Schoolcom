export type UserRole =
  | "SuperAdmin"
  | "SchoolAdmin"
  | "Teacher"
  | "Parent";

export interface AuthUser {
  id: string;
  email?: string;
  phone?: string;
  firstName: string;
  lastName: string;
  roles: UserRole[];
  preferredLanguage: "en" | "ar";
  tenantId: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
  user: AuthUser;
}

export function getDashboardPath(roles: UserRole[]): string {
  if (roles.includes("SuperAdmin") || roles.includes("SchoolAdmin")) {
    return "/(app)/admin";
  }
  if (roles.includes("Teacher")) {
    return "/(app)/teacher";
  }
  return "/(app)/parent";
}

export function isAdmin(roles: UserRole[]): boolean {
  return roles.includes("SchoolAdmin") || roles.includes("SuperAdmin");
}
