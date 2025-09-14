import { Routes } from "@angular/router";
import { Profile } from "../profile/profile";
import { LibrariesList } from "../../library/libraries-list/libraries-list";

export const dashboardRoutes : Routes = [
    {path: "profile", component: Profile},
    {path: "libraries", component: LibrariesList}
];