import { APITester } from "./APITester";
//import "./index.css";

import logo from "./logo.svg";
import reactLogo from "./react.svg";
import {createBrowserRouter, Outlet, RouterProvider} from "react-router";
import {Logind} from "@/Logind.tsx";

export function App() {
  return (
    <RouterProvider router={createBrowserRouter([
        {
            path:"/",
            element:<Outlet/>,
            children: [
                {
                    path:"/",
                    element:<div></div>
                },
                {
                    path:"/logind",
                    element:<Logind/>
                }
            ]
        }
    ])}/>
  );
}

export default App;
