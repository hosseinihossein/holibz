import { ActivatedRoute, RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { libraryRoutes } from './library/library.routes';
import { userAccountRoutes } from './user-account/user-account.routes';
import { inject, NgModule } from '@angular/core';
import { adminRoutes } from './admin/admin.routes';

export const routes: Routes = [
    {path: "", component: Home},
    //{path: "angular/:path", redirectTo: (activatedRoute)=> activatedRoute.params["path"]},
    //{path: "angular", redirectTo: ""},
    ...userAccountRoutes,
    ...libraryRoutes,
    ...adminRoutes
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { anchorScrolling: 'enabled', scrollOffset: [0, 64] })],
  exports: [RouterModule]
})
export class AppRoutingModule {}