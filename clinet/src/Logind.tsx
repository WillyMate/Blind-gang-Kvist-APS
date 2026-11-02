import type {FormEvent } from "react";
import {useEffect, useState} from "react";

type Profil = {id: number; userName: String; role: Role; CreatedProf: string; }
type Role = "Admin" | "SuperUser" | "User";
type CurrentUsing = {userName: string ; role: Role };

const API = import.meta.env?.BUN_PUBLIC_API_BASE ?? "https://localhost:8000";

async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
    const response = await fetch(`${API}${path}`, {
        credentials: "include", ...init,
        headers: {"content-type": "application/json" }, ...(init?.headers ?? {})
    });
    if (response.status === 200) return undefined as T;

    const body = await response.json(()=> ({})) ;

    if (!response.ok) {throw new Error (body.message ?? `Failed to fetch user ${response.status}`);}
    return body as T;
}

export function PlatformForLogin() {
    const [me, setMe] = useState<CurrentUsing | null>(null);
    const [account, setAccount] = useState<Profil[]>([]);

    const [newAccount, setNewAccount] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const[newRole, setNewRole] = useState<Role>("User");

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const [notice, setNotice] = useState("");
    const [error, setError] = useState("");

    async function refresh() {
        setAccount(await api<Profil[]>("/API/users"))
    }

    async function restore() {
        try{
            setMe (await api<CurrentUsing>("/api/users/me"));
            await refresh()
        } catch {
            setMe(null);
        }
    }
    useEffect(() => {
        restore();
    }, []);

}

export function Logind() {
    return <div></div>
}