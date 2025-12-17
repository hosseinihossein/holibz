import { Routes } from "@angular/router";
import { UsersList } from "./identity-admin/users-list/users-list";
import { identityAdminGuard } from "../guards/identity-admin-guard";
import { BackupAdmin } from "./backup-admin/backup-admin";
import { backupAdminGuard } from "../guards/backup-admin-guard";

export const adminRoutes : Routes = [
    {path: "UsersList", component: UsersList, canActivate: [identityAdminGuard]},
    {path: "BackupAdmin", component: BackupAdmin, canActivate: [backupAdminGuard]},
];