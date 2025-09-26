import { Routes } from "@angular/router";
import { Dashboard } from "./dashboard/dashboard";
import { dashboardRoutes } from "./dashboard/dashboard.routes";
import { Login } from "./login/login";
import { Profile } from "./profile/profile";
import { Signup } from "./signup/signup";
import { authGuard } from "../Guards/auth-guard";

export const userAccountRoutes : Routes = [
    {path: "login", component: Login},
    {path: "signup", component: Signup},
    {path: "profile", component:Profile, canActivate: [authGuard]},
    {path: "dashboard", component: Dashboard, children: [...dashboardRoutes], canActivate: [authGuard]},
];