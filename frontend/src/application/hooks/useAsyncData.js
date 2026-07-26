import { useEffect, useState } from "react";

export function useAsyncData(loader, deps) {
  const [state, setState] = useState({ loading: true, error: "", data: null });
  useEffect(() => {
    let alive = true;
    setState((current) => ({ ...current, loading: true, error: "" }));
    loader()
      .then((data) => alive && setState({ loading: false, error: "", data }))
      .catch((error) => alive && setState({ loading: false, error: error.message, data: null }));
    return () => {
      alive = false;
    };
  }, deps);
  return state;
}
