import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { OrganizationDto, UserDto } from "../lib/api/types";

export type AuthState = {
  token?: string;
  user?: UserDto;
  organization?: OrganizationDto;
  setSession: (payload: { token: string; user: UserDto; organization: OrganizationDto }) => void;
  clear: () => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: undefined,
      user: undefined,
      organization: undefined,
      setSession: (payload) => {
        localStorage.setItem("np_token", payload.token);
        set(payload);
      },
      clear: () => {
        localStorage.removeItem("np_token");
        set({ token: undefined, user: undefined, organization: undefined });
      }
    }),
    { name: "notificapix-auth" }
  )
);
