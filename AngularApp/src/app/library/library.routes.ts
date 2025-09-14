import { Routes } from "@angular/router";
import { LibrariesList } from "./libraries-list/libraries-list";
import { LibraryPage } from "./library-page/library-page";
import { DocumentPage } from "./document-page/document-page";
import { DocumentsList } from "./documents-list/documents-list";
import { ShelfPage } from "./shelf-page/shelf-page";
import { ShelvesList } from "./shelves-list/shelves-list";

export const libraryRoutes : Routes = [
    {path: "libraries", component: LibrariesList},
    {path: "library", component: LibraryPage},
    {path: "shelves", component: ShelvesList},
    {path: "shelf", component: ShelfPage},
    {path: "documents", component: DocumentsList},
    {path: "document", component: DocumentPage}
];