import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ReviewModel } from './review';
import { OwnerModel } from '../services/library-service';
import { CommentModel, NewCommentFormModel } from './comment/comment';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private httpClient = inject(HttpClient);


  requestUserTotalLikes(userGuid:string){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    return this.httpClient.get<{totalNumberOfLikes:number}>(
      "/api/Review/GetUserTotalLikes", {params:httpParams}
    );
  }

  requestReviewModel(subjectGuid:string){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    return this.httpClient.get<ReviewModel>(
      "/api/Review/GetReviewModel", {params:httpParams}
    );
  }

  requestComments(subjectGuid:string, sortedBy?:string, pageIndex?:number, pageSize?:number){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    if(sortedBy){
      httpParams.set("sortedBy",sortedBy);
    }
    if(pageIndex){
      httpParams.set("pageIndex",pageIndex);
    }
    if(pageSize){
      httpParams.set("pageSize",pageSize);
    }
    return this.httpClient.get<CommentModel[]>(
      "/api/Review/GetComments", {params:httpParams}
    );
  }
  postNewComment(formModel:NewCommentFormModel){
    const formData = new FormData();
    formData.append("subjectGuid", formModel.parentSubjectGuid);
    formData.append("text", formModel.text);
    
    return this.httpClient.post<CommentModel>(
      "/api/Review/SubmitNewComment", formData
    );
  }

  requestToggleLike(subjectGuid:string){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    return this.httpClient.post<{numberOfLikes:number}>(
      "/api/Review/ToggleLike", null, {params:httpParams}
    );
  }

  requestLikedUserList(subjectGuid:string, bunch:number, filter?:string|null){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    httpParams.set("bunch", bunch);
    if(filter?.trim()){
      httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Review/GetLikedUserList", {params:httpParams}
    );
  }
  requestThumbsUpUserList(subjectGuid:string, bunch:number, filter?:string|null){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    httpParams.set("bunch", bunch);
    if(filter?.trim()){
      httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Review/GetThumbsUpUserList", {params:httpParams}
    );
  }
  requestThumbsDownUserList(subjectGuid:string, bunch:number, filter?:string|null){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    httpParams.set("bunch", bunch);
    if(filter?.trim()){
      httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Review/GetThumbsDownUserList", {params:httpParams}
    );
  }


}
