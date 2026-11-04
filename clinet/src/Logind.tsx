import type { FormEvent } from "react";
import { useEffect, useState } from "react";
import {
    login,
    logout,
    getCurrentUser,
    isLoggedIn,
    getUsers,
    createUser,
    updateUser,
    deleteUser,
    canManageUsers,
    canManageRole,
    type Role,
    type UserSummary,
} from "./api";

const ROLES: Role[] = ["User", "SuperUser", "Admin"];

export function PlatformForLogin() {
    const [me, setMe] = useState<UserSummary | null>(null);
    const [accounts, setAccounts] = useState<UserSummary[]>([]);

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const [newAccount, setNewAccount] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [newRole, setNewRole] = useState<Role>("User");

    const [editingId, setEditingId] = useState<string | null>(null);
    const [editName, setEditName] = useState("");
    const [editRole, setEditRole] = useState<Role>("User");

    const [notice, setNotice] = useState("");
    const [error, setError] = useState("");

    async function refresh() {
        setAccounts(await getUsers());
    }


    useEffect(() => {
        if (!isLoggedIn()) return;
        setMe(getCurrentUser());
        refresh().catch(() => {
            logout();
            setMe(null);
        });
    }, []);

    async function signIn(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        setError("");
        setNotice("");

        try {
            const user = await login(username, password);
            setMe(user);
            setPassword("");
            await refresh();
        } catch (ex) {
            setError(ex instanceof Error ? ex.message : "Something went wrong");
        }
    }

    function signOut() {
        logout();
        setAccounts([]);
        setMe(null);
        setError("");
        setNotice("Logged out");
    }

    async function handleCreate(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        setError("");
        setNotice("");

        try {
            await createUser(newAccount, newPassword, newRole);
            setNewAccount("");
            setNewPassword("");
            setNewRole("User");
            setNotice("Account created");
            await refresh();
        } catch (ex) {
            setError(ex instanceof Error ? ex.message : "Couldn't create account");
        }
    }

    function startEdit(u: UserSummary) {
        setEditingId(u.userId);
        setEditName(u.userName);
        setEditRole(u.role);
    }

    async function saveEdit(userId: string) {
        setError("");
        setNotice("");

        try {
            await updateUser(userId, editName, editRole);
            setEditingId(null);
            setNotice("Account updated");
            await refresh();
        } catch (ex) {
            setError(ex instanceof Error ? ex.message : "Couldn't update account");
        }
    }

    async function handleDelete(u: UserSummary) {
        if (!window.confirm(`Delete ${u.userName}?`)) return;
        setError("");
        setNotice("");

        try {
            await deleteUser(u.userId);
            setNotice("Account deleted");
            await refresh();
        } catch (ex) {
            setError(ex instanceof Error ? ex.message : "Couldn't delete account");
        }
    }

    const manager = canManageUsers(me);
    const assignableRoles = ROLES.filter(r => canManageRole(me, r));

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
                                   required maxLength={64} />
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
                                    {manager && <th>Actions</th>}
                                </tr>
                                </thead>
                                <tbody>
                                {accounts.map(a => {
                                    const editable = canManageRole(me, a.role);
                                    const editing = editingId === a.userId;

                                    return (
                                        <tr key={a.userId}>
                                            <td>
                                                {editing ? (
                                                    <input value={editName}
                                                           onChange={e => setEditName(e.target.value)} />
                                                ) : a.userName}
                                            </td>
                                            <td>
                                                {editing ? (
                                                    <select value={editRole}
                                                            onChange={e => setEditRole(e.target.value as Role)}>
                                                        {assignableRoles.map(r => (
                                                            <option key={r} value={r}>{r}</option>
                                                        ))}
                                                    </select>
                                                ) : a.role}
                                            </td>
                                            {manager && (
                                                <td>
                                                    {editable && (editing ? (
                                                        <>
                                                            <button type="button" className="quiet-button"
                                                                    onClick={() => void saveEdit(a.userId)}>Save</button>
                                                            <button type="button" className="quiet-button"
                                                                    onClick={() => setEditingId(null)}>Cancel</button>
                                                        </>
                                                    ) : (
                                                        <>
                                                            <button type="button" className="quiet-button"
                                                                    onClick={() => startEdit(a)}>Edit</button>
                                                            {a.userId !== me.userId && (
                                                                <button type="button" className="quiet-button"
                                                                        onClick={() => void handleDelete(a)}>Delete</button>
                                                            )}
                                                        </>
                                                    ))}
                                                </td>
                                            )}
                                        </tr>
                                    );
                                })}
                                </tbody>
                            </table>
                        </div>
                    </section>

                    {manager && (
                        <section className="create-account">
                            <h3>Create an account</h3>
                            <form onSubmit={handleCreate}>
                                <label>Username
                                    <input value={newAccount} onChange={e => setNewAccount(e.target.value)}
                                           required minLength={3} maxLength={64} />
                                </label>
                                <label>Password (8 characters minimum)
                                    <input type="password" value={newPassword}
                                           onChange={e => setNewPassword(e.target.value)}
                                           autoComplete="new-password" required minLength={8} />
                                </label>
                                <label>Role
                                    <select value={newRole}
                                            onChange={e => setNewRole(e.target.value as Role)}>
                                        {assignableRoles.map(r => (
                                            <option key={r} value={r}>{r}</option>
                                        ))}
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

// Keep the old export name, in case App.tsx imports Logind
export const Logind = PlatformForLogin;