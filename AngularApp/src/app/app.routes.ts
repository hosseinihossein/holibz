import { ActivatedRoute, Routes } from '@angular/router';
import { Home } from './home/home';
import { libraryRoutes } from './library/library.routes';
import { userAccountRoutes } from './user-account/user-account.routes';
import { inject } from '@angular/core';

export const routes: Routes = [
    {path: "", component: Home},
    //{path: "angular/:path", redirectTo: (activatedRoute)=> activatedRoute.params["path"]},
    //{path: "angular", redirectTo: ""},
    ...userAccountRoutes,
    ...libraryRoutes,
];
