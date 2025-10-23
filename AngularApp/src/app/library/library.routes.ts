import { Routes } from "@angular/router";
import { LibrariesList } from "./libraries-list/libraries-list";
import { LibraryPage } from "./library-page/library-page";
import { DocumentPage } from "./document-page/document-page";
import { DocumentsList } from "./documents-list/documents-list";
import { ShelfPage } from "./shelf-page/shelf-page";
import { ShelvesList } from "./shelves-list/shelves-list";

export const libraryRoutes : Routes = [
    {path: "libraries/:userGuid", component: LibrariesList},
    {path: "libraries", component: LibrariesList},//uses identityService.userModel to get the current user data
    {path: "library/:guid", component: LibraryPage},
    {path: "library", component: LibraryPage},//uses libararyService.currentLibraryModel to get the data from selected libraryCard
    {path: "shelves/:userGuid", component: ShelvesList},
    {path: "shelf", component: ShelfPage},
    {path: "documents", component: DocumentsList},
    {path: "document", component: DocumentPage}
];