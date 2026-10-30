import { APITester } from "./APITester";
//import "./index.css";

import logo from "./logo.svg";
import reactLogo from "./react.svg";
import {createBrowserRouter, Outlet, RouterProvider} from "react-router";

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
                }
            ]
        }
    ])}/>
  );
}

export default App;
