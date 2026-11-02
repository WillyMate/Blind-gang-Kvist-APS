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
    const [newRole, setNewRole] = useState<Role>("User");

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const [notice, setNotice] = useState("");
    const [error, setError] = useState("");

    async function refresh() {
        setAccount(await api<Profil[]>("/API/users"))
    }

    async function restore() {
        try {
            setMe(await api<CurrentUsing>("/api/users/me"));
            await refresh()
        } catch {
            setMe(null);
        }
    }

    useEffect(() => {
        restore();
    }, []);

    async function signIn(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        setError("")

        try {
            await api("/API/users/signin", {
                method: "POST",
                body: JSON.stringify({username, password}),
            });
            setMe(await api<CurrentUsing>("/api/users/me"));
            setPassword("");
            await refresh();
        } catch (ex) {
            setError(ex instanceof Error ? ex.message : "Something went wrong");
        }
    }

    async function signOut() {
        try {
            await api("/API/users/signout", {
                method: "POST",
                body: "{}"
            });
        } finally {
            setAccount([]);
            setMe(null);
            setNotice("Logged out");
        }
    }

    async function CreateProfile(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        setError("");
        setNotice("");

        try {
            await api("/API/users", {
                method: "POST",
                body: JSON.stringify({
                    username: newAccount,
                    password: newPassword,
                    role: newRole,
                }),
            });

            setNewAccount("");
            setNewPassword("");
            setNewRole("User");
            setNotice("Created Account");
            await refresh();

        } catch (ex) {
            setError(ex instanceof Error ? ex.message : "Couldn't create account");
        }
    }
    return (
        <main className="login-platform">
            <header>
                <p className="eyebrow">Kvist &amp; Bjælke ApS</p>
                <h1>Login platform</h1>
                <p>Secure account access with role-based permissions.</p>
            </header>

            {error && <p className="feedback error" role="alert">{error}</p>}
            {notice && <p className="feedback" role="status">{notice}</p>}

            {!me ? (
                <section className="auth-card">
                    <h2>Sign in</h2>
                    <form onSubmit={signIn}>
                        <label>Username
                            <input autoComplete="username" value={username}
                                   onChange={e => setUsername(e.target.value)}
                                   required minLength={3} maxLength={64} />
                        </label>
                        <label>Password
                            <input type="password" autoComplete="current-password" value={password}
                                   onChange={e => setPassword(e.target.value)} required />
                        </label>
                        <button>Sign in</button>
                    </form>
                </section>
            ) : (
                <section className="account-card">
                    <div className="account-header">
                        <div>
                            <h2>Welcome, {me.userName}</h2>
                            <span className="role-badge">{me.role}</span>
                        </div>
                        <button type="button" className="quiet-button" onClick={signOut}>Sign out</button>
                    </div>

                    <section className="user-table">
                        <div className="section-header">
                            <h3>User accounts</h3>
                            <button type="button" className="quiet-button"
                                    onClick={() => void refresh()}>Refresh</button>
                        </div>
                        <div className="table-scroll">
                            <table>
                                <thead>
                                <tr>
                                    <th>Username</th>
                                    <th>Role</th>
                                    <th>Created (UTC)</th>
                                </tr>
                                </thead>
                                <tbody>
                                {account.map(a => (
                                    <tr key={a.id}>
                                        <td>{String(a.userName)}</td>
                                        <td>{a.role}</td>
                                        <td>{new Date(a.CreatedProf).toLocaleString()}</td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>
                    </section>

                    {(me.role === "Admin" || me.role === "SuperUser") && (
                        <section className="create-account">
                            <h3>Create an account</h3>
                            <form onSubmit={CreateProfile}>
                                <label>Username
                                    <input value={newAccount} onChange={e => setNewAccount(e.target.value)}
                                           required minLength={3} maxLength={64} />
                                </label>
                                <label>Password (12 characters minimum)
                                    <input type="password" value={newPassword}
                                           onChange={e => setNewPassword(e.target.value)}
                                           autoComplete="new-password" required minLength={12} />
                                </label>
                                <label>Role
                                    <select value={newRole}
                                            onChange={e => setNewRole(e.target.value as Role)}
                                            disabled={me.role !== "Admin"}>
                                        <option value="User">User</option>
                                        <option value="SuperUser">SuperUser</option>
                                        <option value="Admin">Admin</option>
                                    </select>
                                </label>
                                <button>Create account</button>
                            </form>
                        </section>
                    )}
                </section>
            )}
        </main>
    );
}

export function Logind() {
    return <div></div>
}


