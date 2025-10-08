import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SplashService {
  private showSplashSubject = new BehaviorSubject<boolean>(true);
  public showSplash$ = this.showSplashSubject.asObservable();

  hideSplash() {
    setTimeout(() => {
      this.showSplashSubject.next(false);
    }, 3000);
  }

  showSplash() {
    this.showSplashSubject.next(true);
  }
}