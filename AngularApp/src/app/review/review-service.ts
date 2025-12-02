import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ReviewModel } from './review';
import { OwnerModel } from '../services/library-service';
import { CommentModel, NewCommentFormModel, NewReplyFormModel } from './comment/comment';

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

  requestComments(subjectGuid:string, orderBy?:string|null, pageIndex?:number, pageSize?:number){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    if(orderBy){
      httpParams.set("orderBy",orderBy);
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
  requestReplies(commentGuid:string, bunchIndex:number=0){
    let httpParams = new HttpParams().set("commentGuid",commentGuid);
    httpParams.set("bunchIndex", bunchIndex);
    return this.httpClient.get<CommentModel[]>(
      "/api/Review/GetReplies", {params:httpParams}
    );
  }
  postNewComment(formModel:NewCommentFormModel){
    const formData = new FormData();
    formData.append("ParentSubjectGuid", formModel.parentSubjectGuid);
    formData.append("Text", formModel.text);
    
    return this.httpClient.post<CommentModel>(
      "/api/Review/SubmitNewComment", formData
    );
  }
  postNewReply(formModel:NewReplyFormModel){
    const formData = new FormData();
    formData.append("ParentCommentGuid", formModel.parentCommentGuid);
    formData.append("Text", formModel.text);
    
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
  requestToggleThumbsUp(commentGuid:string){
    let httpParams = new HttpParams().set("commentGuid",commentGuid);
    return this.httpClient.post<{numberOfThumbUps:number}>(
      "/api/Review/ToggleThumbsUp", null, {params:httpParams}
    );
  }
  requestToggleThumbsDown(commentGuid:string){
    let httpParams = new HttpParams().set("commentGuid",commentGuid);
    return this.httpClient.post<{numberOfThumbDowns:number}>(
      "/api/Review/ToggleThumbsDown", null, {params:httpParams}
    );
  }

  requestLikedUserList(subjectGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("subjectGuid",subjectGuid);
    httpParams.set("bunchIndex", bunchIndex);
    if(filter?.trim()){
      httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Review/GetLikedUserList", {params:httpParams}
    );
  }
  requestThumbsUpUserList(commentGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("commentGuid",commentGuid);
    httpParams.set("bunchIndex", bunchIndex);
    if(filter?.trim()){
      httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Review/GetThumbsUpUserList", {params:httpParams}
    );
  }
  requestThumbsDownUserList(commentGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("commentGuid",commentGuid);
    httpParams.set("bunchIndex", bunchIndex);
    if(filter?.trim()){
      httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Review/GetThumbsDownUserList", {params:httpParams}
    );
  }


}
