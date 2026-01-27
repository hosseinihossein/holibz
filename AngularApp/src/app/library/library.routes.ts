import { Routes } from "@angular/router";
//import { LibrariesList } from "./libraries-list/libraries-list";
import { LibraryPage } from "./library-page/library-page";
import { DocumentPage } from "./document-page/document-page";
import { ShelfPage } from "./shelf-page/shelf-page";
import { NewShelfForm } from "./new-shelf-form/new-shelf-form";
import { NewDocumentForm } from "./new-document-form/new-document-form";
import { NewLibraryForm } from "./new-library-form/new-library-form";
import { authGuard } from "../guards/auth-guard";
import { Search } from "./search/search";

export const libraryRoutes : Routes = [
    {path: "library/new", component: NewLibraryForm, canActivate: [authGuard]},
    {path: "library/:libraryGuid", component: LibraryPage},
    {path: "shelf/new/:libraryGuid", component: NewShelfForm, canActivate: [authGuard]},
    {path: "shelf/new", component: NewShelfForm, canActivate: [authGuard]},
    {path: "shelf/:shelfGuid", component: ShelfPage},
    {path: "document/new/:shelfGuid", component: NewDocumentForm, canActivate: [authGuard]},
    {path: "document/new", component: NewDocumentForm, canActivate: [authGuard]},
    {path: "document/:documentGuid", component: DocumentPage},
    {path: "search", component: Search},
];