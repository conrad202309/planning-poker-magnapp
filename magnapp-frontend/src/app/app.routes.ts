import { Routes } from '@angular/router';
import { userSetupGuard } from './core/guards/user-setup.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/setup',
    pathMatch: 'full'
  },
  {
    path: 'setup',
    loadComponent: () => import('./features/user-setup/user-setup.component').then(c => c.UserSetupComponent)
  },
  {
    path: 'sessions',
    loadChildren: () => import('./features/session-management/session-management.module').then(m => m.SessionManagementModule),
    canActivate: [userSetupGuard]
  },
  {
    path: 'session/:id',
    loadChildren: () => import('./features/boardroom/boardroom.module').then(m => m.BoardroomModule),
    canActivate: [userSetupGuard]
  },
  {
    path: '**',
    redirectTo: '/setup'
  }
];
