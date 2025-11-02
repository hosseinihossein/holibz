import { Routes } from "@angular/router";
import { Profile } from "../profile/profile";
import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { UserAccountManager } from "../user-account-manager/user-account-manager";
import { authGuard } from "../../guards/auth-guard";

export const dashboardRoutes : Routes = [
    //{path: "profile", component: Profile, canActivate: [authGuard]},
    //{path: "libraries", component: LibrariesList}
    {path: "UserAccountManager", component: UserAccountManager, canActivate: [authGuard]}
];