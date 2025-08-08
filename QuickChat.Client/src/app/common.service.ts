import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CommonService {

  error: string | null = null;
  constructor() { }
  public handleApiError(err: any, defaultMessage: string = 'An error occurred') {
    let errorMsg: string;
    console.error('full_error:', err);

    if (err.error && err.error.message) {
      errorMsg = err.error.message;
    } else if (typeof err.error === 'string') {
      errorMsg = err.error;
    } else {
      errorMsg = defaultMessage;
    }

    this.error = errorMsg;
    alert(errorMsg);
  }

}
