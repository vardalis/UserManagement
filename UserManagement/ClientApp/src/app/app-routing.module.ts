import { NgModule } from '@angular/core';
import { RouterModule, Routes, PreloadAllModules } from '@angular/router';

import { PageNotFoundComponent } from './shared/page-not-found/page-not-found.component';
import { HomeComponent } from './home/home.component';
// import { AuthGuard } from './core/auth.guard';

const routes: Routes = [
  {
    path:
      'users',
    loadChildren: './users/users.module#UsersModule'
    //data: { allowedRoles: ['admin'] },
    //canLoad: [AuthGuard]
  },
  { path: '', component: HomeComponent },
  { path: '**', component: PageNotFoundComponent }
]

@NgModule({
  declarations: [],
  imports: [
    RouterModule.forRoot(
      routes,
      {
        enableTracing: false, // <-- debugging purposes only
        preloadingStrategy: PreloadAllModules
      } 
    )    
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule { }
