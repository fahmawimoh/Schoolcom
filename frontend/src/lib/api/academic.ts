import { apiFetch } from "@/lib/api/client";

function buildQuery(params: Record<string, string | number | boolean | undefined>) {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") search.set(key, String(value));
  }
  const qs = search.toString();
  return qs ? `?${qs}` : "";
}

// ── School Classes ──────────────────────────────────────────────────────────

export interface SchoolClassSectionItem {
  id: string;
  nameEn: string;
  nameAr: string;
}

export interface SchoolClass {
  id: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string | null;
  descriptionAr?: string | null;
  sortOrder: number;
  isActive: boolean;
  sectionCount: number;
  sections: SchoolClassSectionItem[];
  studentCount: number;
  createdAt: string;
}

export interface SchoolClassListResponse {
  items: SchoolClass[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateSchoolClassPayload {
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  sortOrder: number;
  isActive?: boolean;
  sectionIds: string[];
}

export interface UpdateSchoolClassPayload {
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  sortOrder: number;
  isActive: boolean;
  sectionIds: string[];
}

export const schoolClassesApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    sortDesc?: boolean;
  } = {}) =>
    apiFetch<SchoolClassListResponse>(
      `/admin/school-classes${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        search: params.search,
        isActive: params.isActive,
        sortBy: params.sortBy,
        sortDesc: params.sortDesc,
      })}`
    ),

  getById: (id: string) =>
    apiFetch<SchoolClass>(`/admin/school-classes/${id}`),

  create: (payload: CreateSchoolClassPayload) =>
    apiFetch<SchoolClass>("/admin/school-classes", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateSchoolClassPayload) =>
    apiFetch<SchoolClass>(`/admin/school-classes/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/school-classes/${id}`, { method: "DELETE" }),
};

// ── Sections ────────────────────────────────────────────────────────────────

export interface Section {
  id: string;
  nameEn: string;
  nameAr: string;
  descriptionEn?: string | null;
  descriptionAr?: string | null;
  isActive: boolean;
  linkedClassCount: number;
  createdAt: string;
}

export interface SectionListResponse {
  items: Section[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ClassSectionOption {
  classSectionId: string;
  sectionId: string;
  sectionNameEn: string;
  sectionNameAr: string;
}

export interface CreateSectionPayload {
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  isActive?: boolean;
}

export interface UpdateSectionPayload {
  nameEn: string;
  nameAr: string;
  descriptionEn?: string;
  descriptionAr?: string;
  isActive: boolean;
}

export const sectionsApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    sortDesc?: boolean;
  } = {}) =>
    apiFetch<SectionListResponse>(
      `/admin/sections${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        search: params.search,
        isActive: params.isActive,
        sortBy: params.sortBy,
        sortDesc: params.sortDesc,
      })}`
    ),

  getBySchoolClass: (schoolClassId: string) =>
    apiFetch<ClassSectionOption[]>(
      `/admin/sections/by-school-class/${schoolClassId}`
    ),

  getById: (id: string) => apiFetch<Section>(`/admin/sections/${id}`),

  create: (payload: CreateSectionPayload) =>
    apiFetch<Section>("/admin/sections", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateSectionPayload) =>
    apiFetch<Section>(`/admin/sections/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/sections/${id}`, { method: "DELETE" }),
};

// ── Room Assignments ──────────────────────────────────────────────────────────

export interface RoomAssignment {
  id: string;
  classSectionId: string;
  schoolClassId: string;
  schoolClassNameEn: string;
  schoolClassNameAr: string;
  sectionId: string;
  sectionNameEn: string;
  sectionNameAr: string;
  roomId: string;
  roomNameEn: string;
  roomNameAr: string;
  roomCode?: string | null;
  createdAt: string;
}

export interface RoomAssignmentListResponse {
  items: RoomAssignment[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ClassSectionLookup {
  classSectionId: string;
  schoolClassId: string;
  schoolClassNameEn: string;
  schoolClassNameAr: string;
  sectionId: string;
  sectionNameEn: string;
  sectionNameAr: string;
}

export interface CreateRoomAssignmentPayload {
  classSectionId: string;
  roomId: string;
}

export interface UpdateRoomAssignmentPayload {
  classSectionId: string;
  roomId: string;
}

export const roomAssignmentsApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    schoolClassId?: string;
    sectionId?: string;
    roomId?: string;
  } = {}) =>
    apiFetch<RoomAssignmentListResponse>(
      `/admin/room-assignments${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        schoolClassId: params.schoolClassId,
        sectionId: params.sectionId,
        roomId: params.roomId,
      })}`
    ),

  getClassSectionOptions: (schoolClassId?: string) =>
    apiFetch<ClassSectionLookup[]>(
      `/admin/room-assignments/class-section-options${buildQuery({
        schoolClassId,
      })}`
    ),

  create: (payload: CreateRoomAssignmentPayload) =>
    apiFetch<RoomAssignment>("/admin/room-assignments", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateRoomAssignmentPayload) =>
    apiFetch<RoomAssignment>(`/admin/room-assignments/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/room-assignments/${id}`, { method: "DELETE" }),
};

// ── Subjects ──────────────────────────────────────────────────────────────────

export interface Subject {
  id: string;
  nameEn: string;
  nameAr: string;
  code?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface SubjectListResponse {
  items: Subject[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateSubjectPayload {
  nameEn: string;
  nameAr: string;
  code?: string;
  isActive?: boolean;
}

export interface UpdateSubjectPayload {
  nameEn: string;
  nameAr: string;
  code?: string;
  isActive: boolean;
}

export const subjectsApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    sortDesc?: boolean;
  } = {}) =>
    apiFetch<SubjectListResponse>(
      `/admin/subjects${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        search: params.search,
        isActive: params.isActive,
        sortBy: params.sortBy,
        sortDesc: params.sortDesc,
      })}`
    ),

  getById: (id: string) => apiFetch<Subject>(`/admin/subjects/${id}`),

  create: (payload: CreateSubjectPayload) =>
    apiFetch<Subject>("/admin/subjects", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateSubjectPayload) =>
    apiFetch<Subject>(`/admin/subjects/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/subjects/${id}`, { method: "DELETE" }),
};

// ── Rooms ─────────────────────────────────────────────────────────────────────

export interface Room {
  id: string;
  nameEn: string;
  nameAr: string;
  code?: string | null;
  capacity?: number | null;
  isActive: boolean;
  schoolClassId?: string | null;
  sectionId?: string | null;
  classSectionId?: string | null;
  createdAt: string;
}

export interface RoomListResponse {
  items: Room[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateRoomPayload {
  nameEn: string;
  nameAr: string;
  capacity?: number;
  isActive?: boolean;
  schoolClassId: string;
  sectionId: string;
}

export interface UpdateRoomPayload {
  nameEn: string;
  nameAr: string;
  capacity?: number;
  isActive: boolean;
  schoolClassId: string;
  sectionId: string;
}

export const roomsApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    sortDesc?: boolean;
  } = {}) =>
    apiFetch<RoomListResponse>(
      `/admin/rooms${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        search: params.search,
        isActive: params.isActive,
        sortBy: params.sortBy,
        sortDesc: params.sortDesc,
      })}`
    ),

  getById: (id: string) => apiFetch<Room>(`/admin/rooms/${id}`),

  create: (payload: CreateRoomPayload) =>
    apiFetch<Room>("/admin/rooms", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateRoomPayload) =>
    apiFetch<Room>(`/admin/rooms/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/rooms/${id}`, { method: "DELETE" }),
};

// ── Timetables ────────────────────────────────────────────────────────────────

export interface TimetableEntry {
  id: string;
  schoolClassId: string;
  schoolClassNameEn: string;
  schoolClassNameAr: string;
  sectionId: string;
  sectionNameEn: string;
  sectionNameAr: string;
  teacherId: string;
  teacherFirstNameEn: string;
  teacherFirstNameAr: string;
  teacherLastNameEn: string;
  teacherLastNameAr: string;
  subjectId: string;
  subjectNameEn: string;
  subjectNameAr: string;
  roomId: string;
  roomNameEn: string;
  roomNameAr: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  createdAt: string;
}

export interface TimetableEntryListResponse {
  items: TimetableEntry[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateTimetableEntryPayload {
  schoolClassId: string;
  sectionId: string;
  teacherId: string;
  subjectId: string;
  roomId: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
}

export interface UpdateTimetableEntryPayload {
  schoolClassId: string;
  sectionId: string;
  teacherId: string;
  subjectId: string;
  roomId: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
}

export const timetablesApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    schoolClassId?: string;
    sectionId?: string;
    teacherId?: string;
    subjectId?: string;
    sortBy?: string;
    sortDesc?: boolean;
  } = {}) =>
    apiFetch<TimetableEntryListResponse>(
      `/admin/timetables${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        schoolClassId: params.schoolClassId,
        sectionId: params.sectionId,
        teacherId: params.teacherId,
        subjectId: params.subjectId,
        sortBy: params.sortBy,
        sortDesc: params.sortDesc,
      })}`
    ),

  getById: (id: string) =>
    apiFetch<TimetableEntry>(`/admin/timetables/${id}`),

  create: (payload: CreateTimetableEntryPayload) =>
    apiFetch<TimetableEntry>("/admin/timetables", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateTimetableEntryPayload) =>
    apiFetch<TimetableEntry>(`/admin/timetables/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/timetables/${id}`, { method: "DELETE" }),
};

// ── Teachers ────────────────────────────────────────────────────────────────

export interface TeacherAssignment {
  id: string;
  schoolClassId: string;
  sectionId?: string | null;
  subjectId: string;
  schoolClassNameEn: string;
  schoolClassNameAr: string;
  sectionNameEn?: string | null;
  sectionNameAr?: string | null;
  subjectNameEn: string;
  subjectNameAr: string;
}

export interface Teacher {
  id: string;
  firstNameEn: string;
  firstNameAr: string;
  lastNameEn: string;
  lastNameAr: string;
  email: string;
  phone: string;
  employeeNumber: string;
  subjectEn: string;
  subjectAr: string;
  isActive: boolean;
  userId?: string | null;
  assignments: TeacherAssignment[];
  createdAt: string;
}

export interface TeacherListResponse {
  items: Teacher[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateTeacherPayload {
  firstNameEn: string;
  firstNameAr: string;
  lastNameEn: string;
  lastNameAr: string;
  email: string;
  phone: string;
  employeeNumber: string;
  subjectEn: string;
  subjectAr: string;
  isActive?: boolean;
  userId?: string;
}

export interface UpdateTeacherPayload {
  firstNameEn: string;
  firstNameAr: string;
  lastNameEn: string;
  lastNameAr: string;
  email: string;
  phone: string;
  employeeNumber: string;
  subjectEn: string;
  subjectAr: string;
  isActive: boolean;
  userId?: string;
}

export interface TeacherAssignmentInput {
  schoolClassId: string;
  sectionId?: string;
  subjectId: string;
}

export interface AssignTeacherPayload {
  assignments: TeacherAssignmentInput[];
}

export const teachersApi = {
  list: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    isActive?: boolean;
    subject?: string;
    sortBy?: string;
    sortDesc?: boolean;
  } = {}) =>
    apiFetch<TeacherListResponse>(
      `/admin/teachers${buildQuery({
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 10,
        search: params.search,
        isActive: params.isActive,
        subject: params.subject,
        sortBy: params.sortBy,
        sortDesc: params.sortDesc,
      })}`
    ),

  getById: (id: string) => apiFetch<Teacher>(`/admin/teachers/${id}`),

  create: (payload: CreateTeacherPayload) =>
    apiFetch<Teacher>("/admin/teachers", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  update: (id: string, payload: UpdateTeacherPayload) =>
    apiFetch<Teacher>(`/admin/teachers/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  setAssignments: (id: string, payload: AssignTeacherPayload) =>
    apiFetch<Teacher>(`/admin/teachers/${id}/assignments`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  delete: (id: string) =>
    apiFetch<void>(`/admin/teachers/${id}`, { method: "DELETE" }),
};
