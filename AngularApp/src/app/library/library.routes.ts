import { Routes } from "@angular/router";
import { LibrariesList } from "./libraries-list/libraries-list";
import { LibraryPage } from "./library-page/library-page";
import { DocumentPage } from "./document-page/document-page";
import { DocumentsList } from "./documents-list/documents-list";
import { ShelfPage } from "./shelf-page/shelf-page";
import { ShelvesList } from "./shelves-list/shelves-list";
import { NewShelfForm } from "./new-shelf-form/new-shelf-form";
import { NewDocumentForm } from "./new-document-form/new-document-form";
import { NewLibraryForm } from "./new-library-form/new-library-form";
import { authGuard } from "../guards/auth-guard";

export const libraryRoutes : Routes = [
    {path: "libraries/:userGuid", component: LibrariesList},
    {path: "libraries", component: LibrariesList},//uses identityService.userModel to get the current user data
    {path: "library/new", component: NewLibraryForm, canActivate: [authGuard]},
    {path: "library/:libraryGuid", component: LibraryPage},
    //{path: "library", component: LibraryPage},//uses libararyService.currentLibraryModel to get the data for the selected libraryCard
    //{path: "shelves/:libraryGuid", component: ShelvesList},// equal to library page
    {path: "shelf/new/:libraryGuid", component: NewShelfForm, canActivate: [authGuard]},
    {path: "shelf/new", component: NewShelfForm, canActivate: [authGuard]},
    {path: "shelf/:shelfGuid", component: ShelfPage},
    //{path: "shelf", component: ShelfPage},//use libraryService.currentShelfModel to get data for the selected shelfCard
    //{path: "documents", component: DocumentsList},// equal to shelf page
    {path: "document/new/:shelfGuid", component: NewDocumentForm, canActivate: [authGuard]},
    {path: "document/new", component: NewDocumentForm, canActivate: [authGuard]},
    {path: "document/:documentGuid", component: DocumentPage},
];