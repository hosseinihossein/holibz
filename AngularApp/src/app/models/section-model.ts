export class SectionModel{
  guid = "";
  type: "h1"|"h2"|"p"|"img"|"code"|"file"|"link"= "h1";
  value = "";
  order = 0;
  title?: string;
  file?: File
}