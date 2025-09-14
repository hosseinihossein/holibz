import { Routes } from '@angular/router';
import { Home } from './home/home';
import { libraryRoutes } from './library/library.routes';
import { userAccountRoutes } from './user-account/user-account.routes';

export const routes: Routes = [
    {path: "", component: Home},
    ...userAccountRoutes,
    ...libraryRoutes,
];
