// clinet/src/api.ts
const API = ""; // same origin, the Bun server proxies /api to the .NET API

export type Role = "User" | "SuperUser" | "Admin";

export interface UserSummary {
    userId: string;
    userName: string;
    role: Role;
}

const TOKEN_KEY = "token";
const USER_KEY = "user";

async function request(path: string, init: RequestInit = {}) {
    const token = sessionStorage.getItem(TOKEN_KEY);

    const res = await fetch(`${API}${path}`, {
        ...init,
        headers: {
            "Content-Type": "application/json",
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
            ...init.headers,
        },
    });

    if (res.status === 401 && token) {
        logout();
        throw new Error("Session expired, please sign in again");
    }
    if (res.status === 403) {
        throw new Error("You do not have permission to do that");
    }
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `Server error (${res.status})`);
    }
    return res;
}

export async function login(userName: string, password: string): Promise<UserSummary> {
    const res = await fetch(`${API}/api/User/Login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, password }),
    });

    if (!res.ok) {
        throw new Error(res.status === 401 ? "Wrong username or password" : "Server error");
    }

    const { token, user } = await res.json();
    sessionStorage.setItem(TOKEN_KEY, token);
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    return user;
}

export function logout() {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
}

export function getCurrentUser(): UserSummary | null {
    const raw = sessionStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as UserSummary) : null;
}

export function isLoggedIn(): boolean {
    return sessionStorage.getItem(TOKEN_KEY) !== null;
}

export async function getUsers(): Promise<UserSummary[]> {
    const res = await request("/api/User/GetUsers");
    return res.json();
}

export async function createUser(
    userName: string,
    password: string,
    role: Role
): Promise<UserSummary> {
    const res = await request("/api/User/AddUser", {
        method: "POST",
        body: JSON.stringify({ userName, password, role }),
    });
    return res.json();
}

export async function updateUser(userId: string, userName: string, role: Role): Promise<void> {
    await request(`/api/User/${encodeURIComponent(userId)}`, {
        method: "PUT",
        body: JSON.stringify({ userName, role }),
    });
}

export async function deleteUser(userId: string): Promise<void> {
    await request(`/api/User/${encodeURIComponent(userId)}`, {
        method: "DELETE",
    });
}

// UI helpers only, the server enforces the real rules
export function canManageUsers(user: UserSummary | null): boolean {
    return user?.role === "Admin" || user?.role === "SuperUser";
}

export function canManageRole(user: UserSummary | null, target: Role): boolean {
    if (!user) return false;
    if (user.role === "Admin") return true;
    return user.role === "SuperUser" && target === "User";
}