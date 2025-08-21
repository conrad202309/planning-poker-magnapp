import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { userSetupGuard } from './user-setup.guard';

describe('userSetupGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => userSetupGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
