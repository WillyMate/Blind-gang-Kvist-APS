import { serve } from "bun";
import index from "./index.html";


const API_URL = process.env.API_URL ?? "http://localhost:5215";

const server = serve({
  routes: {

    "/api/*": async (req) => {
      const url = new URL(req.url);

      const headers = new Headers(req.headers);
      headers.delete("host");

      try {
        const upstream = await fetch(API_URL + url.pathname + url.search, {
          method: req.method,
          headers,
          body:
              req.method === "GET" || req.method === "HEAD"
                  ? undefined
                  : await req.arrayBuffer(),
        });

        return new Response(upstream.body, {
          status: upstream.status,
          headers: {
            "content-type": upstream.headers.get("content-type") ?? "text/plain",
          },
        });
      } catch {
        return new Response("API is not reachable", { status: 502 });
      }
    },

    // Serve index.html
    "/*": index,
  },

  development: process.env.NODE_ENV !== "production" && {
    hmr: true,
    console: true,
  },
});

console.log(`🚀 Server running at ${server.url}`);