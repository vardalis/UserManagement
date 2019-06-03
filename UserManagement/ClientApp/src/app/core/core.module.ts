import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogService } from './dialog.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ]
})
export class CoreModule {

  // Trick to prohibit multiple modules importing the core module
  // https://medium.com/@michelestieven/organizing-angular-applications-f0510761d65a
  constructor(@Optional() @SkipSelf() core: CoreModule) {
    if (core) {
      throw new Error('Core module should only be imported once accross the app!');
    }
  }
}
