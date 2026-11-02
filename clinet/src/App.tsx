import"./index.css";
import {createBrowserRouter, Navigate, RouterProvider} from "react-router";
import {Logind, PlatformForLogin} from "@/Logind.tsx";

const router = createBrowserRouter([
    {
        path: "/",
        element: <Navigate to = "/logind" replace />,
    },
    {
        path:"/logind",
        element: <PlatformForLogin />,
    },
]);

export function App() {
    return <RouterProvider router={router} />;
}
export default App;