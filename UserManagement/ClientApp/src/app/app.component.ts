import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AccountService } from './core/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'ClientApp';

  constructor(
    private accountService: AccountService /* bound in the template */,
    private translate: TranslateService
  )
  {
    // this language will be used as a fallback when a translation isn't found in the current language
    translate.setDefaultLang('el');

    // the lang to use, if the lang isn't available, it will use the current loader to get them
    translate.use('el');
  }

  onChange(languageValue) {
    this.translate.use(languageValue);
  }
}
